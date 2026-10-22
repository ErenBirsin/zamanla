import type { Permission } from '@/shared/auth/permissions'
import { useCurrentUser } from './useCurrentUser'

// düğme ve menüleri gizlemek için; asıl kontrol backend'de, burada sadece görünüm
export function usePermissions() {
  const { data: user } = useCurrentUser()

  return {
    isOwner: user?.role === 'Owner',
    // kullanıcı bilgisi gelene kadar hiçbir şey açılmaz
    can: (permission: Permission) => user?.permissions.includes(permission) ?? false,
  }
}
