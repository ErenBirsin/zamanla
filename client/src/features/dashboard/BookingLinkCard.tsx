import { toast } from 'sonner'

interface BookingLinkCardProps {
  slug: string
  ready: boolean
}

const linkButton =
  'flex h-10 shrink-0 cursor-pointer items-center rounded-full border border-line bg-surface px-4 text-sm font-semibold hover:bg-sand-soft'

export function BookingLinkCard({ slug, ready }: BookingLinkCardProps) {
  // müşterinin randevu aldığı sayfa
  const link = `${window.location.origin}/b/${slug}`
  // ekranda "https://" olmadan, daha kısa görünsün
  const displayLink = link.replace(/^https?:\/\//, '')

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(link)
      toast.success('Link kopyalandı')
    } catch {
      toast.error('Link kopyalanamadı, elle seçip kopyalayabilirsin')
    }
  }

  return (
    <section className="flex min-w-0 flex-col gap-4 rounded-card border border-line bg-surface p-6">
      <div className="flex flex-col gap-1">
        <span className="text-xs font-semibold tracking-wide text-muted">RANDEVU LİNKİN</span>
        <h2 className="text-xl font-bold">
          {ready ? 'Salonun randevuya hazır' : 'Kurulum bitince paylaş'}
        </h2>
      </div>

      <p className="text-sm leading-relaxed text-ink-soft">
        {ready
          ? 'Müşterilerin bu linkten hizmet, personel ve saat seçerek randevu alır. Instagram profiline ya da WhatsApp durumuna ekleyebilirsin.'
          : 'Hizmet ve personel ekleyip çalışma saatlerini girince müşterilerin bu linkten randevu alabilir.'}
      </p>

      <div className="flex flex-wrap items-center gap-2">
        <code
          title={link}
          className="min-w-0 flex-1 truncate rounded-xl border border-line-soft bg-sand-soft px-3 py-2.5 text-sm"
        >
          {displayLink}
        </code>
        <a href={link} target="_blank" rel="noreferrer" className={linkButton}>
          Aç
        </a>
        <button type="button" onClick={copy} className={linkButton}>
          Kopyala
        </button>
      </div>
    </section>
  )
}
