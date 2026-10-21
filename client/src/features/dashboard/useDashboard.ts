import { useQuery } from '@tanstack/react-query'
import { appointmentsQueryKey } from '@/features/calendar/useDayAppointments'
import { getDashboard } from './dashboardApi'

// randevu önekinin altında; takvim değişince (canlı güncelleme dahil) özet de yenilenir
export function useDashboard() {
  return useQuery({
    queryKey: [...appointmentsQueryKey, 'dashboard'],
    queryFn: getDashboard,
    // saati geçen randevu "sıradaki" listeden düşsün
    refetchInterval: 60_000,
  })
}
