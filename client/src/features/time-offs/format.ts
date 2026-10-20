import { addDays, formatClock, minutesOfDay, TIME_ZONE, toDateKey } from '@/features/calendar/time'
import type { TimeOff } from './types'

const dayFormat = new Intl.DateTimeFormat('tr-TR', {
  timeZone: TIME_ZONE,
  day: 'numeric',
  month: 'long',
  weekday: 'long',
})

// "29 Ekim Perşembe" ya da "29 Ekim Perşembe – 31 Ekim Cumartesi"
function formatDays(firstDay: string, lastDay: string) {
  const format = (dateKey: string) => dayFormat.format(new Date(`${dateKey}T12:00:00+03:00`))
  return firstDay === lastDay ? format(firstDay) : `${format(firstDay)} – ${format(lastDay)}`
}

// gece yarısından gece yarısına olan izin tüm gün sayılır
export function describeTimeOff(timeOff: TimeOff) {
  const firstDay = toDateKey(timeOff.startAt)
  const allDay = minutesOfDay(timeOff.startAt) === 0 && minutesOfDay(timeOff.endAt) === 0

  if (allDay) {
    return {
      date: formatDays(firstDay, addDays(toDateKey(timeOff.endAt), -1)),
      time: 'Tüm gün',
    }
  }

  return {
    date: formatDays(firstDay, toDateKey(timeOff.endAt)),
    time: `${formatClock(timeOff.startAt)} – ${formatClock(timeOff.endAt)}`,
  }
}
