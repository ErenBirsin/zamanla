import { http } from '@/shared/api/http'
import type { Result } from '@/shared/models/result'
import type { Dashboard } from './types'

export async function getDashboard() {
  const { data } = await http.get<Result<Dashboard>>('/dashboard')
  return data.data!
}
