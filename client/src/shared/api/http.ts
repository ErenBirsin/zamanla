import axios, { isAxiosError } from 'axios'
import type { Result } from '@/shared/models/result'

export const http = axios.create({
  baseURL: '/api',
  timeout: 15_000,
})

export function getErrorMessages(error: unknown): string[] {
  if (isAxiosError<Result<unknown>>(error)) {
    if (!error.response) {
      return ['Sunucuya ulaşılamıyor. Lütfen bağlantınızı kontrol edin.']
    }

    const messages = error.response.data?.errorMessages
    if (messages && messages.length > 0) {
      return messages
    }
  }

  return ['Beklenmeyen bir hata oluştu.']
}
