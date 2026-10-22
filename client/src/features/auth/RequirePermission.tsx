import { Navigate, Outlet } from 'react-router'
import type { Permission } from '@/shared/auth/permissions'
import { useCurrentUser } from './useCurrentUser'

// adresi elle yazan yetkisiz personel genel bakışa döner
export function RequirePermission({ permission }: { permission: Permission }) {
  const { data: user, isPending } = useCurrentUser()

  if (isPending) return null
  return user?.permissions.includes(permission) ? <Outlet /> : <Navigate to="/app" replace />
}
