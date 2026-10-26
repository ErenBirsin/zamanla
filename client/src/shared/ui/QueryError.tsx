import { getErrorMessages } from '@/shared/api/http'

interface QueryErrorProps {
  error: unknown
  onRetry: () => void
}

// sorgu hata verince boş liste gösterilmez; "henüz kayıt yok" yanıltıcı olur
export function QueryError({ error, onRetry }: QueryErrorProps) {
  return (
    <div
      role="alert"
      className="flex max-w-xl flex-col items-start gap-2 rounded-card border border-clay/30 bg-clay-soft/40 p-6"
    >
      <h2 className="text-lg font-bold">Veriler yüklenemedi</h2>
      <p className="text-sm text-ink-soft">{getErrorMessages(error)[0]}</p>
      <button
        type="button"
        onClick={onRetry}
        className="mt-1 h-10 cursor-pointer rounded-full border border-line bg-surface px-4 text-sm font-semibold hover:bg-sand-soft"
      >
        Tekrar dene
      </button>
    </div>
  )
}
