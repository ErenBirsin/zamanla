import { http } from '@/shared/api/http'
import type { Result } from '@/shared/models/result'
import type { PublicSalon } from './types'

export async function getPublicSalon(slug: string) {
  const { data } = await http.get<Result<PublicSalon>>(`/public/${slug}`)
  return data.data!
}
