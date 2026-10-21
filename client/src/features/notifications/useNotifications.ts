import { useQuery } from '@tanstack/react-query'
import { getNotifications, getUnreadCount } from './notificationsApi'

// liste ve sayı aynı önekle; yeni bildirim gelince ikisi birlikte yenilenir
export const notificationsQueryKey = ['notifications']

// liste sadece zil açıkken çekilir
export function useNotifications(enabled: boolean) {
  return useQuery({
    queryKey: [...notificationsQueryKey, 'list'],
    queryFn: getNotifications,
    enabled,
  })
}

export function useUnreadCount() {
  return useQuery({ queryKey: [...notificationsQueryKey, 'unread'], queryFn: getUnreadCount })
}
