import { useCurrentUser } from '@/features/auth/useCurrentUser'
import { formatDayTitle, todayKey } from '@/features/calendar/time'
import { ApiStatusCard } from '@/features/system/ApiStatusCard'
import { BookingLinkCard } from './BookingLinkCard'
import { SetupChecklist } from './SetupChecklist'
import { TodaySummary } from './TodaySummary'
import { UpcomingList } from './UpcomingList'
import { useDashboard } from './useDashboard'
import { useSetupProgress } from './useSetupProgress'

export function DashboardPage() {
  const { data: user } = useCurrentUser()
  const { steps, completed, isLoading } = useSetupProgress()
  const { data: dashboard, isPending: dashboardPending } = useDashboard()
  const setupDone = completed === steps.length

  return (
    <>
      <header className="flex flex-col gap-1.5">
        <h1 className="text-3xl font-bold">
          {user ? `Merhaba, ${user.firstName}` : 'Genel bakış'}
        </h1>
        <p className="text-muted">
          {setupDone || isLoading
            ? `${formatDayTitle(todayKey())} · günün özeti`
            : 'Birkaç adımda salonunu online randevuya hazırlayalım.'}
        </p>
      </header>

      {/* veri gelmeden kart gösterilmez; yoksa adımlar bir an "bekliyor" görünüp sonra işaretlenir */}
      {!isLoading && !setupDone && <SetupChecklist steps={steps} completed={completed} />}

      {dashboardPending ? (
        <p className="text-sm text-muted">Özet yükleniyor…</p>
      ) : (
        dashboard && (
          <>
            <TodaySummary today={dashboard.today} />
            <div className="max-w-3xl">
              <UpcomingList appointments={dashboard.upcoming} />
            </div>
          </>
        )
      )}

      <div className="grid max-w-3xl gap-5 sm:grid-cols-2">
        {user && <BookingLinkCard slug={user.tenantSlug} ready={setupDone} />}
        <ApiStatusCard />
      </div>
    </>
  )
}
