import { useCurrentUser } from '@/features/auth/useCurrentUser'
import { ApiStatusCard } from '@/features/system/ApiStatusCard'
import { BookingLinkCard } from './BookingLinkCard'
import { SetupChecklist } from './SetupChecklist'
import { useSetupProgress } from './useSetupProgress'

export function DashboardPage() {
  const { data: user } = useCurrentUser()
  const { steps, completed, isLoading } = useSetupProgress()
  const setupDone = completed === steps.length

  return (
    <>
      <header className="flex flex-col gap-1.5">
        <h1 className="text-3xl font-bold">
          {user ? `Merhaba, ${user.firstName}` : 'Genel bakış'}
        </h1>
        <p className="text-muted">
          {setupDone || isLoading
            ? 'Randevular ve günün özeti burada görünecek.'
            : 'Birkaç adımda salonunu online randevuya hazırlayalım.'}
        </p>
      </header>

      {/* veri gelmeden kart gösterilmez; yoksa adımlar bir an "bekliyor" görünüp sonra işaretlenir */}
      {!isLoading && !setupDone && <SetupChecklist steps={steps} completed={completed} />}

      <div className="grid max-w-3xl gap-5 sm:grid-cols-2">
        {user && <BookingLinkCard slug={user.tenantSlug} ready={setupDone} />}
        <ApiStatusCard />
      </div>
    </>
  )
}
