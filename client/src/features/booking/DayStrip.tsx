import { type ReactNode, useState } from 'react'
import { todayKey } from '@/features/calendar/time'
import { formatMonthYear, formatWeekdayShort } from './format'

interface DayStripProps {
  days: string[]
  value: string
  isOpen: (day: string) => boolean
  onChange: (day: string) => void
}

const PAGE_SIZE = 7

export function DayStrip({ days, value, isOpen, onChange }: DayStripProps) {
  // seçili gün hangi haftadaysa oradan başlar
  const [page, setPage] = useState(() => Math.max(0, Math.floor(days.indexOf(value) / PAGE_SIZE)))
  const visible = days.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE)
  const lastPage = Math.ceil(days.length / PAGE_SIZE) - 1
  const today = todayKey()

  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <span className="text-[15px] font-bold first-letter:uppercase">
          {formatMonthYear(visible[0])}
        </span>
        <div className="flex gap-2">
          <PageButton label="Önceki hafta" disabled={page === 0} onClick={() => setPage(page - 1)}>
            <path d="m15 18-6-6 6-6" />
          </PageButton>
          <PageButton
            label="Sonraki hafta"
            disabled={page === lastPage}
            onClick={() => setPage(page + 1)}
          >
            <path d="m9 18 6-6-6-6" />
          </PageButton>
        </div>
      </div>
      <div className="grid grid-cols-7 gap-1.5">
        {visible.map((day) => {
          const open = isOpen(day)
          const selected = day === value
          return (
            <button
              key={day}
              type="button"
              disabled={!open}
              aria-pressed={selected}
              onClick={() => onChange(day)}
              className={`flex h-14 cursor-pointer flex-col items-center justify-center rounded-xl border text-xs disabled:cursor-default disabled:border-dashed disabled:text-muted/50 ${selected ? 'border-ink bg-ink text-white' : 'border-line bg-surface'}`}
            >
              <span className={selected ? 'text-white/80' : 'text-muted'}>
                {day === today ? 'Bugün' : formatWeekdayShort(day)}
              </span>
              <span className="text-[17px] font-bold">{Number(day.slice(8))}</span>
            </button>
          )
        })}
      </div>
    </div>
  )
}

interface PageButtonProps {
  label: string
  disabled: boolean
  onClick: () => void
  children: ReactNode
}

function PageButton({ label, disabled, onClick, children }: PageButtonProps) {
  return (
    <button
      type="button"
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
      className="flex size-9 cursor-pointer items-center justify-center rounded-full border border-line bg-surface disabled:cursor-default disabled:opacity-40"
    >
      <svg
        width="16"
        height="16"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2.2"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden
      >
        {children}
      </svg>
    </button>
  )
}
