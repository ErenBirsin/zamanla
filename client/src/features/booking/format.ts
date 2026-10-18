import { TIME_ZONE, formatClock, toDateKey } from '@/features/calendar/time'

// günün ortası; saat dilimi yüzünden bir önceki güne kaymasın
const noon = (dateKey: string) => new Date(`${dateKey}T12:00:00+03:00`)

const weekdayShort = new Intl.DateTimeFormat('tr-TR', { timeZone: TIME_ZONE, weekday: 'short' })
const weekdayLong = new Intl.DateTimeFormat('tr-TR', { timeZone: TIME_ZONE, weekday: 'long' })
const dayMonth = new Intl.DateTimeFormat('tr-TR', {
  timeZone: TIME_ZONE,
  day: 'numeric',
  month: 'long',
})
const monthYear = new Intl.DateTimeFormat('tr-TR', {
  timeZone: TIME_ZONE,
  month: 'long',
  year: 'numeric',
})

// "Per"
export const formatWeekdayShort = (dateKey: string) => weekdayShort.format(noon(dateKey))

// "Ekim 2026"
export const formatMonthYear = (dateKey: string) => monthYear.format(noon(dateKey))

// "15 Ekim Perşembe"
export const formatDayLong = (dateKey: string) =>
  `${dayMonth.format(noon(dateKey))} ${weekdayLong.format(noon(dateKey))}`

// "Per 15 Ekim · 14:00"
export function formatSlot(startAt: string) {
  const dateKey = toDateKey(startAt)
  return `${formatWeekdayShort(dateKey)} ${dayMonth.format(noon(dateKey))} · ${formatClock(startAt)}`
}
