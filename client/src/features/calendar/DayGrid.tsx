import { staffColorClasses } from '@/features/staff/staffOptions'
import type { Staff } from '@/features/staff/types'
import { AppointmentCard } from './AppointmentCard'
import { dayOfWeek, dayRange, minutesOfDay, minutesToLabel, timeToMinutes, todayKey } from './time'
import type { Appointment } from './types'
import { useNow } from './useNow'
import type { MouseEvent } from 'react'
import type { TimeOff } from '@/features/time-offs/types'

const HOUR_HEIGHT = 96
const PX_PER_MINUTE = HOUR_HEIGHT / 60
const COLUMN_MIN_WIDTH = 170
const MIN_CARD_HEIGHT = 15 * PX_PER_MINUTE - 2

interface Range {
  start: number
  end: number
}

interface Column {
  id: string
  name: string
  color: Staff['color']
  ranges: Range[]
  appointments: Appointment[]
}

interface DayGridProps {
  dateKey: string
  staff: Staff[]
  appointments: Appointment[]
  onEmptyClick?: (staffId: string, minutes: number) => void
  // verilmezse bütün sütunlara randevu eklenebilir
  canAddFor?: (staffId: string) => boolean
  selectedId: string | null
  onSelect: (id: string) => void
  timeOffs: TimeOff[]
}

// takvim o gün en erken açılan ve en geç kapanan personele göre çizilir
function gridBounds(columns: Column[]): Range {
  const starts = columns.flatMap((column) => [
    ...column.ranges.map((range) => range.start),
    ...column.appointments.map((appointment) => minutesOfDay(appointment.startAt)),
  ])
  const ends = columns.flatMap((column) => [
    ...column.ranges.map((range) => range.end),
    ...column.appointments.map((appointment) => minutesOfDay(appointment.endAt)),
  ])
  if (starts.length === 0) return { start: 9 * 60, end: 18 * 60 }

  return {
    start: Math.floor(Math.min(...starts) / 60) * 60,
    end: Math.ceil(Math.max(...ends) / 60) * 60,
  }
}

// çalışma aralıklarının dışında kalan kısımlar taranır
function closedBlocks(ranges: Range[], bounds: Range) {
  if (ranges.length === 0) return [{ ...bounds, label: 'Çalışmıyor' }]

  const blocks: (Range & { label: string })[] = []
  let cursor = bounds.start
  ranges.forEach((range, index) => {
    if (range.start > cursor) {
      blocks.push({ start: cursor, end: range.start, label: index === 0 ? 'Mesai dışı' : 'Mola' })
    }
    cursor = Math.max(cursor, range.end)
  })
  if (cursor < bounds.end) blocks.push({ start: cursor, end: bounds.end, label: 'Mesai dışı' })
  return blocks
}

// izinler ve salonun kapalı olduğu saatler; önceki ya da sonraki güne taşan kısım kesilir
function timeOffBlocks(timeOffs: TimeOff[], staffId: string, dateKey: string, bounds: Range) {
  const day = dayRange(dateKey)
  return timeOffs
    .filter((timeOff) => timeOff.staffId === null || timeOff.staffId === staffId)
    .map((timeOff) => {
      const start =
        Date.parse(timeOff.startAt) <= Date.parse(day.start) ? 0 : minutesOfDay(timeOff.startAt)
      const end =
        Date.parse(timeOff.endAt) >= Date.parse(day.end) ? 24 * 60 : minutesOfDay(timeOff.endAt)
      return {
        id: timeOff.id,
        start: Math.max(start, bounds.start),
        end: Math.min(end, bounds.end),
        label: timeOff.staffId === null ? 'Salon kapalı' : 'İzinli',
        note: timeOff.note,
      }
    })
    .filter((block) => block.end > block.start)
}

function initials(name: string) {
  return name
    .split(' ')
    .map((part) => part[0])
    .join('')
    .slice(0, 2)
    .toLocaleUpperCase('tr')
}

export function DayGrid({
  dateKey,
  staff,
  appointments,
  timeOffs,
  onEmptyClick,
  canAddFor,
  selectedId,
  onSelect,
}: DayGridProps) {
  const now = useNow()
  const weekday = dayOfWeek(dateKey)

  const columns: Column[] = staff.map((member) => ({
    id: member.id,
    name: `${member.firstName} ${member.lastName}`,
    color: member.color,
    ranges: member.workingHours
      .filter((hour) => hour.day === weekday)
      .map((hour) => ({ start: timeToMinutes(hour.startTime), end: timeToMinutes(hour.endTime) }))
      .sort((a, b) => a.start - b.start),
    appointments: appointments.filter((appointment) => appointment.staffId === member.id),
  }))

  const bounds = gridBounds(columns)
  const height = (bounds.end - bounds.start) * PX_PER_MINUTE
  const hours = Array.from(
    { length: (bounds.end - bounds.start) / 60 + 1 },
    (_, index) => bounds.start + index * 60,
  )
  const gridTemplateColumns = `64px repeat(${columns.length}, minmax(${COLUMN_MIN_WIDTH}px, 1fr))`

  const nowMinutes = minutesOfDay(now)
  const showNow = dateKey === todayKey() && nowMinutes >= bounds.start && nowMinutes <= bounds.end
  const toTop = (minutes: number) => (minutes - bounds.start) * PX_PER_MINUTE

  // boş alana tıklanan yer 15 dakikaya yuvarlanır; kart ve taralı alan tıklamaları sayılmaz
  const canAdd = (staffId: string) => onEmptyClick !== undefined && (canAddFor?.(staffId) ?? true)

  // boş alana tıklanan yer 15 dakikaya yuvarlanır; kart ve taralı alan tıklamaları sayılmaz
  const handleColumnClick = (staffId: string, event: MouseEvent<HTMLDivElement>) => {
    if (!onEmptyClick || !canAdd(staffId) || event.target !== event.currentTarget) return
    const offset = event.clientY - event.currentTarget.getBoundingClientRect().top
    const minutes = bounds.start + Math.floor(offset / PX_PER_MINUTE / 15) * 15
    onEmptyClick(staffId, minutes)
  }

  return (
    <section
      aria-label="Günlük takvim"
      className="overflow-x-auto rounded-card border border-line bg-surface"
    >
      <div style={{ minWidth: 64 + columns.length * COLUMN_MIN_WIDTH }}>
        <div className="grid border-b border-line" style={{ gridTemplateColumns }}>
          <div />
          {columns.map((column) => (
            <div
              key={column.id}
              className="flex min-w-0 items-center gap-2.5 border-l border-line-soft px-3 py-3.5"
            >
              <span
                className={`flex size-8.5 shrink-0 items-center justify-center rounded-full text-xs font-bold ${staffColorClasses[column.color]}`}
              >
                {initials(column.name)}
              </span>
              <span className="flex min-w-0 flex-col">
                <span className="truncate text-sm font-semibold">{column.name}</span>
                <span className="text-xs text-muted">{column.appointments.length} randevu</span>
              </span>
            </div>
          ))}
        </div>

        <div className="relative grid py-3" style={{ gridTemplateColumns }}>
          <div className="relative" style={{ height }}>
            {hours.map((minutes) => (
              <span
                key={minutes}
                className="absolute right-2.5 -translate-y-1/2 text-[11px] text-muted"
                style={{ top: toTop(minutes) }}
              >
                {minutesToLabel(minutes)}
              </span>
            ))}
          </div>

          {columns.map((column) => (
            <div
              key={column.id}
              className={`relative border-l border-line-soft ${canAdd(column.id) ? 'cursor-pointer' : ''}`}
              onClick={(event) => handleColumnClick(column.id, event)}
              style={{
                height,
                // saat başında çizgi, yarım saatte daha açık çizgi
                backgroundImage: `repeating-linear-gradient(to bottom, var(--color-line) 0, var(--color-line) 1px, transparent 1px, 
                transparent ${HOUR_HEIGHT / 2}px, var(--color-line-soft) ${HOUR_HEIGHT / 2}px, var(--color-line-soft) ${HOUR_HEIGHT / 2 + 1}px,
                 transparent ${HOUR_HEIGHT / 2 + 1}px, transparent ${HOUR_HEIGHT}px)`,
              }}
            >
              {/* geçen saatler hafif soluk; mesai dışı ve izin alanları bunun üstünde kalır */}
              {showNow && (
                <div
                  aria-hidden
                  className="pointer-events-none absolute inset-x-0 top-0 bg-sand/60"
                  style={{ height: toTop(nowMinutes) }}
                />
              )}

              {closedBlocks(column.ranges, bounds).map((block) => (
                <div
                  key={block.start}
                  className="absolute inset-x-1 rounded-[10px] px-2.5 py-2 text-xs font-semibold text-muted"
                  style={{
                    top: toTop(block.start) + 1,
                    height: (block.end - block.start) * PX_PER_MINUTE - 2,
                    backgroundImage:
                      'repeating-linear-gradient(135deg, var(--color-line-soft) 0, var(--color-line-soft) 6px, var(--color-sand-soft) 6px, var(--color-sand-soft) 12px)',
                  }}
                >
                  {block.label}
                </div>
              ))}

              {timeOffBlocks(timeOffs, column.id, dateKey, bounds).map((block) => (
                <div
                  key={block.id}
                  className="absolute inset-x-1 flex flex-col gap-0.5 overflow-hidden rounded-[10px] border border-clay/20 px-2.5 py-2 text-xs text-clay-dark"
                  style={{
                    top: toTop(block.start) + 1,
                    height: (block.end - block.start) * PX_PER_MINUTE - 2,
                    backgroundImage:
                      'repeating-linear-gradient(135deg, var(--color-clay-soft) 0, var(--color-clay-soft) 6px, var(--color-sand-soft) 6px, var(--color-sand-soft) 12px)',
                  }}
                >
                  <span className="font-semibold">{block.label}</span>
                  {block.note && <span className="truncate">{block.note}</span>}
                </div>
              ))}

              {column.appointments.map((appointment) => {
                const start = minutesOfDay(appointment.startAt)
                const end = minutesOfDay(appointment.endAt)
                return (
                  <AppointmentCard
                    key={appointment.id}
                    appointment={appointment}
                    top={toTop(start) + 1}
                    height={Math.max((end - start) * PX_PER_MINUTE - 2, MIN_CARD_HEIGHT)}
                    selected={appointment.id === selectedId}
                    onSelect={() => onSelect(appointment.id)}
                  />
                )
              })}
            </div>
          ))}

          {/* rozetin ortası tam şu anki saate gelsin */}
          {showNow && (
            <div
              aria-hidden
              className="pointer-events-none absolute inset-x-0 z-0 flex -translate-y-1/2 items-center"
              style={{ top: toTop(nowMinutes) + 12 }}
            >
              <span className="flex w-14 justify-end pr-1">
                <span className="rounded-full bg-clay px-1.5 py-0.5 text-[11px] font-bold text-white">
                  {minutesToLabel(nowMinutes)}
                </span>
              </span>
              <span className="ml-1 size-2 rounded-full bg-clay" />
              <span className="h-0.5 flex-1 bg-clay" />
            </div>
          )}
        </div>
      </div>
    </section>
  )
}
