import { type Permission, PERMISSIONS } from '@/shared/auth/permissions'
import { useCurrentUser } from './useCurrentUser'

// düğme ve menüleri gizlemek için; asıl kontrol backend'de, burada sadece görünüm
export function usePermissions() {
  const { data: user } = useCurrentUser()
  // kullanıcı bilgisi gelene kadar hiçbir şey açılmaz
  const can = (permission: Permission) => user?.permissions.includes(permission) ?? false

  return {
    isOwner: user?.role === 'Owner',
    // personelin takvimdeki kaydı; sahipte null
    staffId: user?.staffId ?? null,
    can,
    // backend'deki AccessService ile aynı kural: kendi randevusu ya da "manage-all" yetkisi
    canManageAppointment: (staffId: string) =>
      user?.staffId === staffId || can(PERMISSIONS.appointmentsManageAll),
  }
}
