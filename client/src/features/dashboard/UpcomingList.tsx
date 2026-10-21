import { Link } from 'react-router'
import { formatClock, todayKey } from '@/features/calendar/time'
import type { DashboardUpcoming } from './types'

interface UpcomingListProps {
  appointments: DashboardUpcoming[]
}

export function UpcomingList({ appointments }: UpcomingListProps) {
  return (
    <section className="flex min-w-0 flex-col gap-4 rounded-card border border-line bg-surface p-6">
      <div className="flex items-baseline justify-between gap-3">
        <h2 className="text-xl font-bold">Sıradaki randevular</h2>
        <Link
          to={`/app/calendar?date=${todayKey()}`}
          className="text-sm font-semibold text-clay-dark hover:underline"
        >
          Takvime git
        </Link>
      </div>

      {appointments.length === 0 ? (
        <p className="text-sm text-muted">Bugün için bekleyen randevu kalmadı.</p>
      ) : (
        <ul className="flex flex-col">
          {appointments.map((appointment) => (
            <li
              key={appointment.id}
              className="flex items-start gap-4 border-t border-line-soft py-3.5 first:border-t-0 first:pt-0 last:pb-0"
            >
              <span className="w-12 shrink-0 font-semibold tabular-nums">
                {formatClock(appointment.startAt)}
              </span>
              <div className="flex min-w-0 flex-1 flex-col gap-0.5">
                <span className="flex items-center gap-2 font-semibold">
                  <span className="truncate">{appointment.customerFullName}</span>
                  {appointment.isOnline && (
                    <span className="rounded-full bg-ink px-1.5 text-[10px] text-white">
                      Online
                    </span>
                  )}
                </span>
                <span className="truncate text-sm text-muted">
                  {appointment.serviceNames.join(', ')} · {appointment.staffFullName}
                </span>
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
