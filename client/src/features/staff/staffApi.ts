import { http } from '@/shared/api/http'
import type { ODataResponse } from '@/shared/models/odata'
import type { Result } from '@/shared/models/result'
import type { StaffColor } from './staffOptions'
import type { Staff, WorkingHour } from './types'

export interface StaffRequest {
  firstName: string
  lastName: string
  color: StaffColor
  workingHours: WorkingHour[]
  salonServiceIds: string[]
}

export async function getStaff() {
  const { data } = await http.get<ODataResponse<Staff>>('/odata/staff', {
    params: { $orderby: 'firstName,lastName' },
  })
  return data.value
}

export async function createStaff(request: StaffRequest) {
  const { data } = await http.post<Result<string>>('/staff', request)
  return data.data!
}

export async function updateStaff(id: string, request: StaffRequest, isActive: boolean) {
  const { data } = await http.put<Result<string>>('/staff', { id, ...request, isActive })
  return data.data!
}

export async function deleteStaff(id: string) {
  const { data } = await http.delete<Result<string>>(`/staff/${id}`)
  return data.data!
}
