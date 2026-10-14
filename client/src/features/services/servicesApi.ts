import { http } from '@/shared/api/http'
import type { ODataResponse } from '@/shared/models/odata'
import type { Result } from '@/shared/models/result'
import type { ServiceValues } from './schemas'
import type { SalonService } from './types'

export async function getServices() {
  const { data } = await http.get<ODataResponse<SalonService>>('/odata/services', {
    params: { $orderby: 'category,name' },
  })
  return data.value
}

// isActive alanı yeni kayıtta yok sayılır, hizmet her zaman aktif başlar
export async function createService(values: ServiceValues) {
  const { data } = await http.post<Result<string>>('/services', values)
  return data.data!
}

export async function updateService(id: string, values: ServiceValues) {
  const { data } = await http.put<Result<string>>('/services', { id, ...values })
  return data.data!
}

export async function deleteService(id: string) {
  const { data } = await http.delete<Result<string>>(`/services/${id}`)
  return data.data!
}
