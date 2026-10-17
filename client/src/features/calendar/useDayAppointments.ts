import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getDayAppointments } from './appointmentsApi'

// geçersiz kılarken bütün günleri kapsayan ortak önek
export const appointmentsQueryKey = ['appointments']

export function useDayAppointments(dateKey: string) {
  return useQuery({
    queryKey: [...appointmentsQueryKey, dateKey],
    queryFn: () => getDayAppointments(dateKey),
    placeholderData: keepPreviousData, // gün değiştirirken eski gün yeni veri gelene kadar ekranda kalır
  })
}
