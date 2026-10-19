import { http } from '@/shared/api/http'
import type { Result } from '@/shared/models/result'
import type { SettingsValues } from './schemas'
import type { Settings } from './types'

export async function getSettings() {
  const { data } = await http.get<Result<Settings>>('/settings')
  return data.data!
}

// formdaki düz alanlar backend'in beklediği adres ve kural nesnelerine çevrilir
export async function updateSettings(values: SettingsValues) {
  const hasAddress = values.city !== ''
  const { data } = await http.put<Result<string>>('/settings', {
    name: values.name,
    address: hasAddress
      ? { city: values.city, district: values.district, fullAddress: values.fullAddress }
      : null,
    phoneNumber: values.phoneNumber.trim() || null,
    bookingPolicy: {
      maxDaysAhead: values.maxDaysAhead,
      cancelHoursBefore: values.cancelHoursBefore,
      maxUpcomingPerPhone: values.maxUpcomingPerPhone,
    },
  })
  return data.data!
}
