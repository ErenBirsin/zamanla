import { Link } from 'react-router'

export function LoginPage() {
  return (
    <div className="flex flex-col gap-6">
      <header className="flex flex-col gap-1.5">
        <h2 className="text-3xl font-bold">Tekrar hoş geldin</h2>
        <p className="text-muted">Salonunun takvimine giriş yap.</p>
      </header>

      <p className="rounded-card border border-dashed border-line p-6 text-sm text-muted">
        Giriş formu bir sonraki adımda gelecek.
      </p>

      <Link to="/register" className="text-sm font-semibold text-clay hover:underline">
        İşletmeni ücretsiz kaydet
      </Link>
    </div>
  )
}
