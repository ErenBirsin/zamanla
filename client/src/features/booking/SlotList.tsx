import { formatClock, minutesOfDay } from '@/features/calendar/time'
import type { AvailabilitySlot } from '@/features/calendar/types'

interface SlotListProps {
  slots: AvailabilitySlot[]
  value: string | null
  onChange: (slot: AvailabilitySlot) => void
}

const groups = [
  { label: 'Sabah', until: 12 * 60 },
  { label: 'Öğle', until: 14 * 60 },
  { label: 'Öğleden sonra', until: 17 * 60 },
  { label: 'Akşam', until: 24 * 60 },
]

export function SlotList({ slots, value, onChange }: SlotListProps) {
  return (
    <div className="flex flex-col gap-4">
      {groups.map((group, index) => {
        const from = index === 0 ? 0 : groups[index - 1].until
        const items = slots.filter((slot) => {
          const minutes = minutesOfDay(slot.startAt)
          return minutes >= from && minutes < group.until
        })
        if (items.length === 0) return null

        return (
          <div key={group.label} className="flex flex-col gap-2">
            <span className="text-xs font-semibold tracking-wide text-muted uppercase">
              {group.label}
            </span>
            <div className="grid grid-cols-4 gap-2">
              {items.map((slot) => {
                const selected = slot.startAt === value
                return (
                  <button
                    key={slot.startAt}
                    type="button"
                    aria-pressed={selected}
                    onClick={() => onChange(slot)}
                    className={`h-11.5 cursor-pointer rounded-xl border text-[15px] font-semibold ${selected ? 'border-clay bg-clay text-white' : 'border-line bg-surface hover:bg-sand-soft'}`}
                  >
                    {formatClock(slot.startAt)}
                  </button>
                )
              })}
            </div>
          </div>
        )
      })}
    </div>
  )
}
