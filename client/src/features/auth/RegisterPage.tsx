import { Link } from 'react-router'

export function RegisterPage() {
  return (
    <div className="flex flex-col gap-6">
      <header className="flex flex-col gap-1.5">
        <h2 className="text-3xl font-bold">İşletmeni kaydet</h2>
        <p className="text-muted">Birkaç dakikada randevu sayfan hazır olsun.</p>
      </header>

      <p className="rounded-card border border-dashed border-line p-6 text-sm text-muted">
        Kayıt formu bir sonraki adımda gelecek.
      </p>

      <Link to="/login" className="text-sm font-semibold text-clay hover:underline">
        Zaten hesabın var mı? Giriş yap
      </Link>
    </div>
  )
}
