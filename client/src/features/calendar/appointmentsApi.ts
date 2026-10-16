import { http } from '@/shared/api/http'
import type { ODataResponse } from '@/shared/models/odata'
import { dayRange } from './time'
import type { Appointment, AppointmentCreateBody, AvailabilitySlot } from './types'
import type { Result } from '@/shared/models/result'

// iptal edilen randevu saati tutmaz, takvimde de gösterilmez
export async function getDayAppointments(dateKey: string) {
  const { start, end } = dayRange(dateKey)
  const { data } = await http.get<ODataResponse<Appointment>>('/odata/appointments', {
    params: {
      $filter: `startAt ge ${start} and startAt lt ${end} and status ne 'İptal Edildi'`,
      $orderby: 'startAt',
    },
  })
  return data.value
}

// staffId boşsa "fark etmez": her saat için o gün en az randevusu olan personel gelir
export async function getAvailability(date: string, salonServiceIds: string[], staffId: string) {
  const { data } = await http.get<Result<AvailabilitySlot[]>>('/appointments/availability', {
    params: { date, salonServiceIds, staffId: staffId || undefined }, // backend listeyi "salonServiceIds=a&salonServiceIds=b" biçiminde bekliyor
    paramsSerializer: { indexes: null },
  })
  return data.data!
}

export async function createAppointment(body: AppointmentCreateBody) {
  const { data } = await http.post<Result<string>>('/appointments', body)
  return data.data!
}

export async function completeAppointment(id: string) {
  const { data } = await http.put<Result<string>>(`/appointments/${id}/complete`)
  return data.data!
}

export async function markAppointmentNoShow(id: string) {
  const { data } = await http.put<Result<string>>(`/appointments/${id}/no-show`)
  return data.data!
}

// neden boşsa backend "Salon tarafından iptal edildi" yazar
export async function cancelAppointment(id: string, reason: string) {
  const { data } = await http.put<Result<string>>(`/appointments/${id}/cancel`, { reason })
  return data.data!
}
