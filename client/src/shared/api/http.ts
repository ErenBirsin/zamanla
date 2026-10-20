import axios, { isAxiosError, type InternalAxiosRequestConfig } from 'axios'
import { useSession } from '@/shared/auth/session'
import type { AuthTokens } from '@/shared/models/auth'
import type { Result } from '@/shared/models/result'

export const http = axios.create({
  baseURL: '/api',
  timeout: 15_000,
})

// bu uçlardan gelen 401 "oturum bitti" anlamına gelmez
const anonymousEndpoints = ['/auth/login', '/auth/register', '/auth/refresh']

type RetryableRequest = InternalAxiosRequestConfig & { _retry?: boolean }

let refreshPromise: Promise<string> | null = null

http.interceptors.request.use((config) => {
  const { accessToken } = useSession.getState()
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

http.interceptors.response.use(undefined, async (error: unknown) => {
  if (!isAxiosError(error) || error.response?.status !== 401) {
    throw error
  }

  const request = error.config as RetryableRequest | undefined
  const { refreshToken } = useSession.getState()
  if (
    !request ||
    request._retry ||
    !refreshToken ||
    anonymousEndpoints.includes(request.url ?? '')
  ) {
    throw error
  }

  // aynı anda 401 alan istekler tek bir yenilemeyi bekliyor
  refreshPromise ??= refreshAccessToken(refreshToken).finally(() => {
    refreshPromise = null
  })

  request._retry = true
  request.headers.Authorization = `Bearer ${await refreshPromise}`
  return http(request)
})

// SignalR istekleri axios'tan geçmez; bağlanırken token'ı buradan alır, süresi dolmak üzereyse önce yenilenir
export async function getFreshAccessToken() {
  const { accessToken, refreshToken } = useSession.getState()
  if (accessToken && !isExpiringSoon(accessToken)) {
    return accessToken
  }
  if (!refreshToken) {
    return ''
  }

  refreshPromise ??= refreshAccessToken(refreshToken).finally(() => {
    refreshPromise = null
  })
  return refreshPromise
}

// token'ın ortasındaki kısım base64 JSON; exp saniye cinsinden bitiş zamanı
function isExpiringSoon(accessToken: string) {
  const payload = accessToken.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
  const { exp } = JSON.parse(atob(payload)) as { exp: number }
  return exp * 1000 - Date.now() < 30_000
}

async function refreshAccessToken(refreshToken: string) {
  try {
    const { data } = await http.post<Result<AuthTokens>>('/auth/refresh', { refreshToken })
    useSession.getState().setTokens(data.data!)
    return data.data!.accessToken
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 401) {
      useSession.getState().clear()
    }
    throw error
  }
}

export function getErrorMessages(error: unknown): string[] {
  if (isAxiosError<Result<unknown>>(error)) {
    if (!error.response) {
      return ['Sunucuya ulaşılamıyor. Lütfen bağlantınızı kontrol edin.']
    }

    const messages = error.response.data?.errorMessages
    if (messages && messages.length > 0) {
      return messages
    }

    // rate limiter cevabı gövdesiz dönüyor, bu yüzden status koduna bakıyoruz
    if (error.response.status === 429) {
      return ['Çok fazla deneme yaptınız. Lütfen bir dakika sonra tekrar deneyin.']
    }

    if (error.response.status === 401) {
      return ['Tekrar giriş yapmalısınız.']
    }
  }

  return ['Beklenmeyen bir hata oluştu.']
}
