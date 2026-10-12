import { useQuery } from '@tanstack/react-query'
import { getCurrentUser } from './authApi'

export function useCurrentUser() {
  return useQuery({
    queryKey: ['current-user'],
    queryFn: getCurrentUser,
    staleTime: 5 * 60 * 1000,
  })
}
