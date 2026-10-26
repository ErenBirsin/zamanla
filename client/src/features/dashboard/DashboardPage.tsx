import { useCurrentUser } from '@/features/auth/useCurrentUser'
import { usePermissions } from '@/features/auth/usePermissions'
import { formatDayTitle, todayKey } from '@/features/calendar/time'
import { BookingLinkCard } from './BookingLinkCard'
import { SetupChecklist } from './SetupChecklist'
import { TodaySummary } from './TodaySummary'
import { UpcomingList } from './UpcomingList'
import { useDashboard } from './useDashboard'
import { useSetupProgress } from './useSetupProgress'
import { WeeklyChart } from './WeeklyChart'
import { BlockSkeleton } from '@/shared/ui/Skeleton'
import { QueryError } from '@/shared/ui/QueryError'

export function DashboardPage() {
  const { data: user } = useCurrentUser()
  const { isOwner } = usePermissions()
  const { steps, completed, isLoading } = useSetupProgress()
  const {
    data: dashboard,
    isPending: dashboardPending,
    isError: dashboardError,
    error,
    refetch,
  } = useDashboard()
  const setupDone = completed === steps.length

  return (
    <>
      <header className="flex flex-col gap-1.5">
        <h1 className="text-3xl font-bold">
          {user ? `Merhaba, ${user.firstName}` : 'Genel bakış'}
        </h1>
        <p className="text-muted">
          {/* kurulum çağrısı sadece sahibe */}
          {setupDone || isLoading || !isOwner
            ? `${formatDayTitle(todayKey())} · günün özeti`
            : 'Birkaç adımda salonunu online randevuya hazırlayalım.'}
        </p>
      </header>

      {/* kurulum adımlarını sahip tamamlar; veri gelmeden kart gösterilmez, yoksa adımlar bir an "bekliyor" görünüp sonra işaretlenir */}
      {isOwner && !isLoading && !setupDone && (
        <SetupChecklist steps={steps} completed={completed} />
      )}

      {dashboardPending ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            {[1, 2, 3, 4].map((card) => (
              <BlockSkeleton key={card} className="h-28" />
            ))}
          </div>
          <div className="grid gap-5 lg:grid-cols-2">
            <BlockSkeleton className="h-72" />
            <BlockSkeleton className="h-72" />
          </div>
        </>
      ) : dashboardError ? (
        <QueryError error={error} onRetry={() => void refetch()} />
      ) : (
        dashboard && (
          <>
            <TodaySummary today={dashboard.today} />
            <div className="grid gap-5 lg:grid-cols-2">
              <UpcomingList appointments={dashboard.upcoming} />
              <WeeklyChart days={dashboard.lastSevenDays} />
            </div>
          </>
        )
      )}
      {user && (
        <div className="max-w-xl">
          <BookingLinkCard slug={user.tenantSlug} ready={setupDone} />
        </div>
      )}
    </>
  )
}
