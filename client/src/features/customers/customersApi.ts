import { http } from '@/shared/api/http'
import type { ODataResponse } from '@/shared/models/odata'
import type { Result } from '@/shared/models/result'
import type { CustomerValues } from './schemas'
import type { Customer } from './types'

// aramada rakamlar varsa telefon, yoksa ad soyad içinde aranır
function toFilter(search: string) {
  const text = search.trim()
  if (!text) return undefined

  if (/^[\d\s()+-]+$/.test(text)) {
    // "0532 123" ve "+90 532" gibi yazımlar kayıtlı biçime ("+90532...") uysun
    const digits = text.replace(/\D/g, '').replace(/^(90|0)/, '')
    return digits ? `contains(phoneNumber,'${digits}')` : undefined
  }

  // OData metninde tek tırnak iki kez yazılır
  return `contains(fullName,'${text.replaceAll("'", "''")}')`
}

export async function getCustomers(search: string) {
  const { data } = await http.get<ODataResponse<Customer>>('/odata/customers', {
    params: { $orderby: 'firstName,lastName', $filter: toFilter(search) },
  })
  return data.value
}

export async function createCustomer(values: CustomerValues) {
  const { data } = await http.post<Result<string>>('/customers', values)
  return data.data!
}

export async function updateCustomer(id: string, values: CustomerValues) {
  const { data } = await http.put<Result<string>>('/customers', { id, ...values })
  return data.data!
}

// kayıt silinmez, kişisel bilgileri temizlenir
export async function deleteCustomer(id: string) {
  const { data } = await http.delete<Result<string>>(`/customers/${id}`)
  return data.data!
}
