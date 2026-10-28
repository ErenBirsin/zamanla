import { dayOfWeek, todayKey } from '@/features/calendar/time'
import { formatPhone } from '@/features/customers/phone'
import { weekDays } from '@/features/staff/staffOptions'
import { formatAddress } from './format'
import { initials, openHoursOnDay } from './hours'
import type { PublicSalon } from './types'

interface SalonInfoCardProps {
  salon: PublicSalon
}

// bilgisayarda solda sabit duran salon bilgisi; telefonda üstteki koyu başlık bu işi görür
export function SalonInfoCard({ salon }: SalonInfoCardProps) {
  const today = dayOfWeek(todayKey())

  return (
    <aside className="sticky top-10 hidden w-80 shrink-0 flex-col gap-6 rounded-[28px] bg-ink p-6 text-sand lg:flex">
      <div className="flex items-center gap-3.5">
        <span className="flex size-13 shrink-0 items-center justify-center rounded-2xl bg-clay font-display text-xl font-bold text-white">
          {initials(salon.name)}
        </span>
        <h2 className="text-2xl leading-tight font-bold">{salon.name}</h2>
      </div>

      {(salon.address || salon.phoneNumber) && (
        <div className="flex flex-col gap-2 text-sm text-[#cfc4b6]">
          {salon.address && <span>{formatAddress(salon.address)}</span>}
          {salon.phoneNumber && (
            <a
              href={`tel:${salon.phoneNumber}`}
              className="w-fit font-semibold text-sand hover:underline"
            >
              {formatPhone(salon.phoneNumber)}
            </a>
          )}
        </div>
      )}

      <div className="flex flex-col gap-2">
        <span className="text-xs font-semibold tracking-wide text-[#cfc4b6]">ÇALIŞMA SAATLERİ</span>
        <ul className="flex flex-col gap-1.5 text-sm">
          {weekDays.map(({ day, label }) => {
            const hours = openHoursOnDay(salon.staff, day)
            return (
              <li
                key={day}
                className={`flex justify-between ${day === today ? 'font-semibold text-sand' : 'text-[#cfc4b6]'}`}
              >
                <span>
                  {label}
                  {day === today && ' · bugün'}
                </span>
                <span className="tabular-nums">{hours ?? 'Kapalı'}</span>
              </li>
            )
          })}
        </ul>
      </div>
    </aside>
  )
}
