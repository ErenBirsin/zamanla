import { Link } from 'react-router'
import { Logo } from '@/shared/ui/Logo'
import { StatusMessage } from '@/shared/ui/StatusMessage'

export function NotFoundPage() {
  return (
    <main className="flex min-h-dvh flex-col items-center justify-center gap-10 px-6 py-10">
      <Logo />
      <div className="w-full max-w-sm">
        <StatusMessage
          icon="search"
          title="Sayfa bulunamadı"
          message="Aradığın sayfa taşınmış ya da hiç var olmamış olabilir."
        >
          <Link
            to="/"
            className="flex h-13 items-center justify-center rounded-full bg-clay text-[15px] font-semibold text-white hover:bg-clay-dark"
          >
            Ana sayfaya dön
          </Link>
        </StatusMessage>
      </div>
    </main>
  )
}
