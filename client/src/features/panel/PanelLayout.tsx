import { NavLink, Outlet } from 'react-router'
import { Logo } from '@/shared/ui/Logo'

const upcomingItems = ['Takvim', 'Randevular', 'Müşteriler', 'Hizmetler', 'Personel', 'Ayarlar']

export function PanelLayout() {
  return (
    <div className="flex min-h-dvh flex-col md:flex-row">
      <aside className="flex flex-col gap-6 border-b border-line bg-sand-soft px-4 py-5 md:w-64 md:shrink-0 md:border-r md:border-b-0 md:py-6">
        <div className="px-2">
          <Logo />
        </div>

        <nav className="flex flex-col gap-0.5">
          <NavLink
            to="/app"
            end
            className={({ isActive }) =>
              `rounded-xl px-3 py-2.5 text-sm font-semibold ${isActive ? 'bg-clay-soft text-clay-dark' : 'text-ink-soft hover:bg-line-soft'}`
            }
          >
            Genel bakış
          </NavLink>
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
      </aside>

      <main className="flex min-w-0 flex-1 flex-col gap-6 px-4 py-6 sm:px-8 sm:py-8">
        <Outlet />
      </main>
    </div>
  )
}
