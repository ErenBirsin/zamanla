import { Link } from 'react-router'
import { Logo } from '@/shared/ui/Logo'

export function NotFoundPage() {
  return (
    <main className="mx-auto flex min-h-dvh max-w-md flex-col items-start justify-center gap-6 px-6">
      <Logo />
      <div className="flex flex-col gap-2">
        <h1 className="text-4xl font-bold">Sayfa bulunamadı</h1>
        <p className="text-muted">Aradığın sayfa taşınmış ya da hiç var olmamış olabilir.</p>
      </div>
      <Link
        to="/"
        className="flex h-11 items-center rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark"
      >
        Ana sayfaya dön
      </Link>
    </main>
  )
}
