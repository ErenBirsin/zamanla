import { formatPrice } from '@/shared/ui/format'
import type { DashboardToday } from './types'

interface TodaySummaryProps {
  today: DashboardToday
}

interface StatProps {
  label: string
  value: string
  detail: string
}

function Stat({ label, value, detail }: StatProps) {
  return (
    <div className="flex flex-col gap-1 rounded-card border border-line bg-surface p-5">
      <span className="text-xs font-semibold tracking-wide text-muted">{label}</span>
      <strong className="font-display text-3xl font-bold tabular-nums">{value}</strong>
      <span className="text-sm text-muted">{detail}</span>
    </div>
  )
}

export function TodaySummary({ today }: TodaySummaryProps) {
  const confirmed = today.appointmentCount - today.completedCount - today.noShowCount

  return (
    <section
      aria-label="Bugünün özeti"
      // ciro kartı yoksa üç kart satırı doldurur
      className={`grid gap-4 sm:grid-cols-2 ${today.expectedRevenue === null ? 'lg:grid-cols-3' : 'lg:grid-cols-4'}`}
    >
      <Stat
        label="BUGÜNKÜ RANDEVU"
        value={String(today.appointmentCount)}
        detail={confirmed > 0 ? `${confirmed} tanesi onaylı` : 'Onaylı randevu kalmadı'}
      />
      {today.expectedRevenue !== null && (
        <Stat
          label="BEKLENEN CİRO"
          value={formatPrice(today.expectedRevenue)}
          detail="Gelmeyenler hariç"
        />
      )}
      <Stat
        label="TAMAMLANAN"
        value={String(today.completedCount)}
        detail={today.noShowCount > 0 ? `${today.noShowCount} müşteri gelmedi` : 'Gelmeyen yok'}
      />
      <Stat
        label="İPTAL"
        value={String(today.cancelledCount)}
        detail="Randevu sayısına dahil değil"
      />
    </section>
  )
}
