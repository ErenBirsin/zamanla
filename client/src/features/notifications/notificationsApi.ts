import { http } from '@/shared/api/http'
import type { ODataResponse } from '@/shared/models/odata'
import type { Result } from '@/shared/models/result'
import type { AppNotification } from './types'

// zilde son 20 bildirim
export async function getNotifications() {
  const { data } = await http.get<ODataResponse<AppNotification>>('/odata/notifications', {
    params: { $orderby: 'createdAt desc', $top: 20 },
  })
  return data.value
}

// liste çekilmeden sadece okunmamış sayısı
export async function getUnreadCount() {
  const { data } = await http.get<ODataResponse<AppNotification>>('/odata/notifications', {
    params: { $filter: 'isRead eq false', $count: true, $top: 0 },
  })
  return data['@odata.count'] ?? 0
}

export async function markNotificationRead(id: string) {
  const { data } = await http.put<Result<string>>(`/notifications/${id}/read`)
  return data.data!
}

export async function markAllNotificationsRead() {
  const { data } = await http.put<Result<string>>('/notifications/read-all')
  return data.data!
}
