import { type ApiHealth, useApiHealth } from './useApiHealth'

const statusView: Record<ApiHealth, { label: string; detail: string; dot: string }> = {
  checking: {
    label: 'Kontrol ediliyor',
    detail: 'API’ye istek gönderildi.',
    dot: 'bg-line',
  },
  healthy: {
    label: 'Çalışıyor',
    detail: 'API ayakta ve SQL Server’a bağlanabiliyor.',
    dot: 'bg-sage',
  },
  unhealthy: {
    label: 'Veritabanına ulaşılamıyor',
    detail: 'API çalışıyor ama SQL Server bağlantısı kurulamadı.',
    dot: 'bg-clay',
  },
  unreachable: {
    label: 'API’ye ulaşılamıyor',
    detail: 'Zamanla.WebAPI projesinin çalıştığından emin ol.',
    dot: 'bg-clay',
  },
}

export function ApiStatusCard() {
  const { status, checkedAt, check } = useApiHealth()
  const view = statusView[status]

  return (
    <section
      aria-live="polite"
      className="flex flex-col gap-5 rounded-card border border-line bg-surface p-6"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex flex-col gap-1">
          <span className="text-xs font-semibold tracking-wide text-muted">BACKEND DURUMU</span>
          <h2 className="text-xl font-bold">{view.label}</h2>
        </div>
        <span className="relative mt-1.5 flex size-3">
          {status === 'checking' && (
            <span className="absolute inline-flex size-full animate-ping rounded-full bg-line" />
          )}
          <span className={`relative inline-flex size-3 rounded-full ${view.dot}`} />
        </span>
      </div>

      <p className="text-sm leading-relaxed text-ink-soft">{view.detail}</p>

      <div className="flex flex-wrap items-center justify-between gap-3">
        <span className="text-xs text-muted">
          {checkedAt
            ? `Son kontrol: ${checkedAt.toLocaleTimeString('tr-TR')}`
            : 'Henüz kontrol edilmedi'}
        </span>
        <button
          type="button"
          onClick={check}
          disabled={status === 'checking'}
          className="h-10 cursor-pointer rounded-full border border-line bg-surface px-4 text-sm font-semibold text-ink transition-colors hover:bg-sand-soft disabled:cursor-default disabled:opacity-50"
        >
          Tekrar dene
        </button>
      </div>
    </section>
  )
}
