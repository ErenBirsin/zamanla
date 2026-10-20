import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getDayTimeOffs, getUpcomingTimeOffs } from './timeOffsApi'

export const timeOffsQueryKey = ['timeOffs']

export function useTimeOffs() {
  return useQuery({ queryKey: timeOffsQueryKey, queryFn: getUpcomingTimeOffs })
}

// takvim için; izin eklenip silinince ortak anahtar sayesinde bu da yenilenir
export function useDayTimeOffs(dateKey: string) {
  return useQuery({
    queryKey: [...timeOffsQueryKey, dateKey],
    queryFn: () => getDayTimeOffs(dateKey),
    placeholderData: keepPreviousData,
  })
}
