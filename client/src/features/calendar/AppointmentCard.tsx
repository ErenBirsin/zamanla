import { staffAccentClasses, staffColorClasses } from '@/features/staff/staffOptions'
import { formatClock } from './time'
import type { Appointment } from './types'

interface AppointmentCardProps {
  appointment: Appointment
  top: number
  height: number
  selected: boolean
  onSelect: () => void
}

export function AppointmentCard({
  appointment,
  top,
  height,
  selected,
  onSelect,
}: AppointmentCardProps) {
  const { status } = appointment
  const time = `${formatClock(appointment.startAt)} – ${formatClock(appointment.endAt)}`
  const services = appointment.salonServices.map((service) => service.name).join(' + ')
  // kart boyuna göre: tek satır (saat + ad), iki satır, üç satır (+ hizmetler)
  const rows = height >= 62 ? 3 : height >= 40 ? 2 : 1

  const colors =
    status === 'Tamamlandı'
      ? 'border border-line bg-surface text-muted'
      : status === 'Gelmedi'
        ? 'border border-dashed border-clay bg-surface text-clay-dark'
        : `border-l-[3px] ${staffColorClasses[appointment.staffColor]} ${staffAccentClasses[appointment.staffColor]} text-ink`

  return (
    <button
      type="button"
      onClick={onSelect}
      aria-pressed={selected}
      aria-label={`${time} ${appointment.customerFullName}, ${services}, ${status}`}
      className={`absolute inset-x-1 z-10 flex cursor-pointer overflow-hidden rounded-lg px-2.5 text-left leading-4 shadow-[0_1px_2px_rgb(30_27_24/0.08)] transition-shadow hover:shadow-[0_4px_12px_rgb(30_27_24/0.12)] ${rows === 1 ? 'items-center gap-1.5' : 'flex-col gap-0.5 py-1.5'} ${colors} ${selected ? 'ring-2 ring-ink' : ''}`}
      style={{ top, height }}
    >
      <span className="flex items-center gap-1.5 text-[11px] font-semibold whitespace-nowrap opacity-85">
        {time}
        {appointment.source === 'Online' && (
          <span className="rounded-full bg-ink px-1.5 text-[10px] text-white">Online</span>
        )}
        {status === 'Tamamlandı' && (
          <span className="flex items-center gap-0.5 rounded-full bg-sage-soft px-1.5 text-[10px] text-sage">
            <svg
              width="10"
              height="10"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="3"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden
            >
              <path d="M20 6 9 17l-5-5" />
            </svg>
            Tamamlandı
          </span>
        )}

        {status === 'Gelmedi' && (
          <span className="rounded-full bg-clay px-1.5 text-[10px] text-white">Gelmedi</span>
        )}
      </span>
      <span className="truncate text-[13px] font-semibold">{appointment.customerFullName}</span>
      {rows === 3 && <span className="truncate text-xs opacity-85">{services}</span>}
    </button>
  )
}
