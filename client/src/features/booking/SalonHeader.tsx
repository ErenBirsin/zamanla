import { todayKey } from '@/features/calendar/time'
import { initials, openHours } from './hours'
import type { PublicSalon } from './types'

interface SalonHeaderProps {
  salon: PublicSalon
}

export function SalonHeader({ salon }: SalonHeaderProps) {
  const hours = openHours(salon.staff, todayKey())

  return (
    <header className="flex flex-col gap-3.5 rounded-b-[28px] bg-ink px-5 pt-7 pb-6.5 text-sand">
      <div className="flex items-center gap-3.5">
        <span className="flex size-13 shrink-0 items-center justify-center rounded-2xl bg-clay font-display text-xl font-bold text-white">
          {initials(salon.name)}
        </span>
        <h1 className="text-[26px] leading-tight font-bold">{salon.name}</h1>
      </div>
      <p className="flex w-fit items-center gap-1.5 rounded-full bg-[#2e2925] px-2.5 py-1.5 text-[13px]">
        <span className={`size-1.75 rounded-full ${hours ? 'bg-[#8db580]' : 'bg-muted'}`} />
        {hours ? `Bugün açık · ${hours}` : 'Bugün kapalı'}
      </p>
    </header>
  )
}
