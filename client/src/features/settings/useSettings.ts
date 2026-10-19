import { useQuery } from '@tanstack/react-query'
import { getSettings } from './settingsApi'

export const settingsQueryKey = ['settings']

export function useSettings() {
  return useQuery({ queryKey: settingsQueryKey, queryFn: getSettings })
}
