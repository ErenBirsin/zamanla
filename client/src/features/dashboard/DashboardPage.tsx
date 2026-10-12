import { useCurrentUser } from '@/features/auth/useCurrentUser'
import { ApiStatusCard } from '@/features/system/ApiStatusCard'

export function DashboardPage() {
  const { data: user } = useCurrentUser()

  return (
    <>
      <header className="flex flex-col gap-1.5">
        <h1 className="text-3xl font-bold">
          {user ? `Merhaba, ${user.firstName}` : 'Genel bakış'}
        </h1>
        <p className="text-muted">Randevular ve günün özeti burada görünecek.</p>
      </header>

      <div className="grid max-w-3xl gap-5 sm:grid-cols-2">
        <ApiStatusCard />
      </div>
    </>
  )
}
