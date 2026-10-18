import { isAxiosError } from 'axios'
import { FormError } from '@/shared/ui/FormError'
import { Logo } from '@/shared/ui/Logo'

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
    <main className="mx-auto flex min-h-dvh max-w-md flex-col items-start justify-center gap-6 px-6">
      <Logo />
      {notFound ? (
        <div className="flex flex-col gap-2">
          <h1 className="text-3xl font-bold">{title}</h1>
          <p className="text-muted">{message}</p>
        </div>
      ) : (
        <FormError error={error} />
      )}
    </main>
  )
}
