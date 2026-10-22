import { useQueryClient } from '@tanstack/react-query'
import { NavLink, Outlet } from 'react-router'
import { useCurrentUser } from '@/features/auth/useCurrentUser'
import { usePermissions } from '@/features/auth/usePermissions'
import { type Permission, PERMISSIONS } from '@/shared/auth/permissions'
import { useSession } from '@/shared/auth/session'
import { Logo } from '@/shared/ui/Logo'
import { useRealtimeUpdates } from '@/shared/realtime/useRealtimeUpdates'
import { NotificationBell } from '@/features/notifications/NotificationBell'

interface NavItem {
  to: string
  label: string
  end: boolean
  // yoksa herkes görür
  permission?: Permission
}

const navItems: NavItem[] = [
  { to: '/app', label: 'Genel bakış', end: true },
  { to: '/app/calendar', label: 'Takvim', end: false },
  { to: '/app/services', label: 'Hizmetler', end: false },
  { to: '/app/staff', label: 'Personel', end: false },
  { to: '/app/customers', label: 'Müşteriler', end: false },
  { to: '/app/settings', label: 'Ayarlar', end: false, permission: PERMISSIONS.settingsManage },
  { to: '/app/time-offs', label: 'İzinler', end: false },
]

const upcomingItems = ['Randevular']

const roleLabels = { Owner: 'İşletme sahibi', Staff: 'Personel' }

export function PanelLayout() {
  const { data: user } = useCurrentUser()
  const { can } = usePermissions()
  const clearSession = useSession((state) => state.clear)
  const queryClient = useQueryClient()
  useRealtimeUpdates()

  const logout = () => {
    queryClient.clear()
    clearSession()
  }

  return (
    <div className="flex min-h-dvh flex-col md:flex-row">
      <aside className="flex flex-col gap-6 border-b border-line bg-sand-soft px-4 py-5 md:w-64 md:shrink-0 md:border-r md:border-b-0 md:py-6">
        <div className="flex items-center justify-between px-2">
          <Logo />
          <NotificationBell />
        </div>

        <div className="flex flex-col gap-0.5 rounded-xl border border-line bg-surface px-3 py-2.5">
          <span className="truncate text-sm font-semibold">{user?.tenantName ?? '…'}</span>
          <span className="truncate text-xs text-muted">zamanla.app/{user?.tenantSlug ?? '…'}</span>
        </div>

        <nav className="flex flex-col gap-0.5">
          {navItems
            .filter((item) => !item.permission || can(item.permission))
            .map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                className={({ isActive }) =>
                  `rounded-xl px-3 py-2.5 text-sm font-semibold ${isActive ? 'bg-clay-soft text-clay-dark' : 'text-ink-soft hover:bg-line-soft'}`
                }
              >
                {item.label}
              </NavLink>
            ))}
          {upcomingItems.map((item) => (
            <span
              key={item}
              className="flex items-center justify-between rounded-xl px-3 py-2.5 text-sm text-muted/70"
            >
              {item}
              <span className="text-[11px] font-semibold tracking-wide">YAKINDA</span>
            </span>
          ))}
        </nav>

        <div className="mt-auto flex items-center justify-between gap-3 px-2">
          <div className="flex min-w-0 flex-col">
            <span className="truncate text-sm font-semibold">
              {user ? `${user.firstName} ${user.lastName}` : '…'}
            </span>
            <span className="text-xs text-muted">{user ? roleLabels[user.role] : ''}</span>
          </div>
          <button
            type="button"
            onClick={logout}
            className="h-9 shrink-0 cursor-pointer rounded-full border border-line bg-surface px-3 text-xs font-semibold hover:bg-line-soft"
          >
            Çıkış
          </button>
        </div>
      </aside>

      <main className="flex min-w-0 flex-1 flex-col gap-6 px-4 py-6 sm:px-8 sm:py-8">
        <Outlet />
      </main>
    </div>
  )
}
