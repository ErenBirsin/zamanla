import { http } from '@/shared/api/http'
import type { ODataResponse } from '@/shared/models/odata'
import type { Result } from '@/shared/models/result'
import type { TimeOff, TimeOffCreateBody } from './types'

// bitmiş izinler listelenmez
export async function getUpcomingTimeOffs() {
  const { data } = await http.get<ODataResponse<TimeOff>>('/odata/timeOffs', {
    params: { $filter: `endAt gt ${new Date().toISOString()}`, $orderby: 'startAt' },
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
