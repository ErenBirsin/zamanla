import { http } from '@/shared/api/http'
import type { ODataResponse } from '@/shared/models/odata'
import type { Result } from '@/shared/models/result'
import type { TimeOff, TimeOffCreateBody } from './types'
import { dayRange } from '@/features/calendar/time'

// bitmiş izinler listelenmez
export async function getUpcomingTimeOffs() {
  const { data } = await http.get<ODataResponse<TimeOff>>('/odata/timeOffs', {
    params: { $filter: `endAt gt ${new Date().toISOString()}`, $orderby: 'startAt' },
  })
  return data.value
}

// o güne denk gelen izinler; birden fazla gün süren izin her günde görünür
export async function getDayTimeOffs(dateKey: string) {
  const { start, end } = dayRange(dateKey)
  const { data } = await http.get<ODataResponse<TimeOff>>('/odata/timeOffs', {
    params: { $filter: `startAt lt ${end} and endAt gt ${start}` },
  })
  return data.value
}

export async function createTimeOff(body: TimeOffCreateBody) {
  const { data } = await http.post<Result<string>>('/time-offs', body)
  return data.data!
}

export async function deleteTimeOff(id: string) {
  const { data } = await http.delete<Result<string>>(`/time-offs/${id}`)
  return data.data!
}
