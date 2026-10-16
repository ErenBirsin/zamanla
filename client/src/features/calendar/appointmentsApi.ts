import { http } from '@/shared/api/http'
import type { ODataResponse } from '@/shared/models/odata'
import { dayRange } from './time'
import type { Appointment } from './types'

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
