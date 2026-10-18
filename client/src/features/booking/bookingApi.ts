import { http } from '@/shared/api/http'
import type { Result } from '@/shared/models/result'
import type { PublicSalon } from './types'
import type { AvailabilitySlot } from '@/features/calendar/types'

export async function getPublicSalon(slug: string) {
  const { data } = await http.get<Result<PublicSalon>>(`/public/${slug}`)
  return data.data!
}

// staffId boşsa "fark etmez": her saat için o gün en az randevusu olan personel gelir
export async function getPublicAvailability(
  slug: string,
  date: string,
  salonServiceIds: string[],
  staffId: string,
) {
  const { data } = await http.get<Result<AvailabilitySlot[]>>(`/public/${slug}/availability`, {
    params: { date, salonServiceIds, staffId: staffId || undefined },
    paramsSerializer: { indexes: null },
  })
  return data.data!
}
