import { useQuery } from '@tanstack/react-query'
import { getAvailability } from './appointmentsApi'
import { appointmentsQueryKey } from './useDayAppointments'

export function useAvailability(date: string, salonServiceIds: string[], staffId: string) {
  return useQuery({
    queryKey: [...appointmentsQueryKey, 'availability', date, salonServiceIds, staffId], // randevular geçersiz kılınınca boş saatler de yenilensin
    queryFn: () => getAvailability(date, salonServiceIds, staffId),
    enabled: salonServiceIds.length > 0,
    meta: { inlineError: true },
  })
}
