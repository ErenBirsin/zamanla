import { useQuery } from '@tanstack/react-query'
import { getRescheduleAvailability } from './bookingApi'
import { publicQueryKey } from './usePublicSalon'

export function useRescheduleAvailability(
  slug: string,
  token: string,
  date: string,
  staffId: string,
) {
  return useQuery({
    queryKey: [...publicQueryKey(slug), 'manage', token, 'availability', date, staffId],
    queryFn: () => getRescheduleAvailability(slug, token, date, staffId),
  })
}
