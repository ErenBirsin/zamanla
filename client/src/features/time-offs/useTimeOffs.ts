import { useQuery } from '@tanstack/react-query'
import { getUpcomingTimeOffs } from './timeOffsApi'

export const timeOffsQueryKey = ['timeOffs']

export function useTimeOffs() {
  return useQuery({ queryKey: timeOffsQueryKey, queryFn: getUpcomingTimeOffs })
}
