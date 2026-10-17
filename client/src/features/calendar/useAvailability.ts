import { useQuery } from '@tanstack/react-query'
import { getAvailability } from './appointmentsApi'
import { appointmentsQueryKey } from './useDayAppointments'

export function useAvailability(
  date: string,
  salonServiceIds: string[],
  staffId: string,
  appointmentId?: string,
) {
  return useQuery({
    // randevular geçersiz kılınınca boş saatler de yenilensin
    queryKey: [
      ...appointmentsQueryKey,
      'availability',
      date,
      salonServiceIds,
      staffId,
      appointmentId,
    ],
    queryFn: () => getAvailability(date, salonServiceIds, staffId, appointmentId),
    enabled: salonServiceIds.length > 0,
    meta: { inlineError: true },
  })
}
