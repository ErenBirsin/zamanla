import { useQuery } from '@tanstack/react-query'
import { getServices } from './servicesApi'

export const servicesQueryKey = ['services']

export function useServices() {
  return useQuery({ queryKey: servicesQueryKey, queryFn: getServices })
}
