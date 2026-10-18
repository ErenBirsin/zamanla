import { useQuery } from '@tanstack/react-query'
import { getPublicAvailability } from './bookingApi'
import { publicQueryKey } from './usePublicSalon'

export function usePublicAvailability(
  slug: string,
  date: string,
  salonServiceIds: string[],
  staffId: string,
) {
  return useQuery({
    queryKey: [...publicQueryKey(slug), 'availability', date, salonServiceIds, staffId],
    queryFn: () => getPublicAvailability(slug, date, salonServiceIds, staffId),
    enabled: salonServiceIds.length > 0,
  })
}
