import { useQuery } from '@tanstack/react-query'
import { getStaff } from './staffApi'

export const staffQueryKey = ['staff']

export function useStaff() {
  return useQuery({ queryKey: staffQueryKey, queryFn: getStaff })
}
