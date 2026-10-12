import { Navigate, Outlet } from 'react-router'
import { useSession } from './session'

export function RequireAuth() {
  const refreshToken = useSession((state) => state.refreshToken)
  return refreshToken ? <Outlet /> : <Navigate to="/login" replace />
}

export function GuestOnly() {
  const refreshToken = useSession((state) => state.refreshToken)
  return refreshToken ? <Navigate to="/app" replace /> : <Outlet />
}
