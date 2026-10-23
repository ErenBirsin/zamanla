import { http } from '@/shared/api/http'
import type { AuthTokens, CurrentUser } from '@/shared/models/auth'
import type { Result } from '@/shared/models/result'
import type { ForgotPasswordValues, LoginValues, RegisterValues } from './schemas'

export async function login(values: LoginValues) {
  const { data } = await http.post<Result<AuthTokens>>('/auth/login', values)
  return data.data!
}

export async function registerBusiness(values: RegisterValues) {
  const { data } = await http.post<Result<AuthTokens>>('/auth/register', values)
  return data.data!
}

export async function forgotPassword(values: ForgotPasswordValues) {
  const { data } = await http.post<Result<string>>('/auth/forgot-password', values)
  return data.data!
}

export async function checkForgotPasswordCode(code: string) {
  const { data } = await http.get<Result<boolean>>(`/auth/check-forgot-password-code/${code}`)
  return data.data!
}

export async function resetPassword(code: string, newPassword: string) {
  const { data } = await http.post<Result<string>>('/auth/reset-password', {
    forgotPasswordCode: code,
    newPassword,
  })
  return data.data!
}

export async function changePassword(currentPassword: string, newPassword: string) {
  const { data } = await http.put<Result<AuthTokens>>('/auth/password', {
    currentPassword,
    newPassword,
  })
  return data.data!
}

export async function getCurrentUser() {
  const { data } = await http.get<Result<CurrentUser>>('/auth/me')
  return data.data!
}
