import { http } from '@/shared/api/http'
import type { Result } from '@/shared/models/result'
import type {
  PublicAppointment,
  PublicAppointmentCreated,
  PublicBookingBody,
  PublicSalon,
} from './types'
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

export async function createPublicAppointment(slug: string, body: PublicBookingBody) {
  const { data } = await http.post<Result<PublicAppointmentCreated>>(
    `/public/${slug}/appointments`,
    body,
  )
  return data.data!
}

export async function getManagedAppointment(slug: string, token: string) {
  const { data } = await http.get<Result<PublicAppointment>>(`/public/${slug}/manage/${token}`)
  return data.data!
}

// saat değiştirirken boş saatler; randevunun kendi saati boş sayılır
export async function getRescheduleAvailability(
  slug: string,
  token: string,
  date: string,
  staffId: string,
) {
  const { data } = await http.get<Result<AvailabilitySlot[]>>(
    `/public/${slug}/manage/${token}/availability`,
    { params: { date, staffId: staffId || undefined } },
  )
  return data.data!
}

export async function cancelManagedAppointment(slug: string, token: string) {
  const { data } = await http.put<Result<string>>(`/public/${slug}/manage/${token}/cancel`)
  return data.data!
}

export async function rescheduleManagedAppointment(
  slug: string,
  token: string,
  staffId: string,
  startAt: string,
) {
  const { data } = await http.put<Result<string>>(`/public/${slug}/manage/${token}/reschedule`, {
    staffId,
    startAt,
  })
  return data.data!
}
