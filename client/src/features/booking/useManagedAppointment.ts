import { useQuery } from '@tanstack/react-query'
import { getManagedAppointment } from './bookingApi'
import { publicQueryKey } from './usePublicSalon'

export function useManagedAppointment(slug: string, token: string) {
  return useQuery({
    queryKey: [...publicQueryKey(slug), 'manage', token],
    queryFn: () => getManagedAppointment(slug, token),
    meta: { inlineError: true },
    retry: false,
  })
}
