import { http } from '@/shared/api/http'
import type { AuthTokens, CurrentUser } from '@/shared/models/auth'
import type { Result } from '@/shared/models/result'
import type { LoginValues, RegisterValues } from './schemas'

export async function login(values: LoginValues) {
  const { data } = await http.post<Result<AuthTokens>>('/auth/login', values)
  return data.data!
}

export async function registerBusiness(values: RegisterValues) {
  const { data } = await http.post<Result<AuthTokens>>('/auth/register', values)
  return data.data!
}

export async function getCurrentUser() {
  const { data } = await http.get<Result<CurrentUser>>('/auth/me')
  return data.data!
}
