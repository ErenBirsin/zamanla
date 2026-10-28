import { isAxiosError } from 'axios'
import { FormError } from '@/shared/ui/FormError'
import { Logo } from '@/shared/ui/Logo'
import { StatusMessage } from '@/shared/ui/StatusMessage'

interface SalonNotFoundProps {
  error: unknown
  title?: string
  message?: string
}

// adres yanlış yazıldıysa ya da salon kapandıysa; diğer hatalar olduğu gibi gösterilir
export function SalonNotFound({
  error,
  title = 'Salon bulunamadı',
  message = 'Bu adreste bir salon yok. Linki salondan tekrar isteyebilirsin.',
}: SalonNotFoundProps) {
  const notFound = isAxiosError(error) && error.response?.status === 404

  return (
    <main className="flex min-h-dvh flex-col items-center justify-center gap-10 px-6 py-10">
      <Logo />
      <div className="w-full max-w-sm">
        {notFound ? (
          <StatusMessage icon="search" title={title} message={message} />
        ) : (
          <StatusMessage icon="alert" title="Sayfa açılamadı">
            <FormError error={error} />
          </StatusMessage>
        )}
      </div>
    </main>
  )
}
