import type { DaySchedule } from './schemas'
import { weekDays } from './staffOptions'
import type { WorkingHour } from './types'

const defaultRange = { start: '09:00', end: '18:00' }

// yeni personel: pazartesi–cumartesi 09:00–18:00, pazar kapalı
export function defaultSchedule(): DaySchedule[] {
  return weekDays.map(({ day }) => ({ day, enabled: day !== 0, ranges: [{ ...defaultRange }] }))
}

// API listesini formun gün satırlarına çevirir
export function toSchedule(workingHours: WorkingHour[]): DaySchedule[] {
  return weekDays.map(({ day }) => {
    const ranges = workingHours
      .filter((hour) => hour.day === day)
      .map((hour) => ({ start: hour.startTime.slice(0, 5), end: hour.endTime.slice(0, 5) }))

    return ranges.length > 0
      ? { day, enabled: true, ranges }
      : { day, enabled: false, ranges: [{ ...defaultRange }] }
  })
}

// formdaki açık günleri API'nin beklediği düz listeye çevirir
export function toWorkingHours(schedule: DaySchedule[]): WorkingHour[] {
  return schedule
    .filter((day) => day.enabled)
    .flatMap((day) =>
      day.ranges.map((range) => ({
        day: day.day,
        startTime: `${range.start}:00`,
        endTime: `${range.end}:00`,
      })),
    )
}

export function formatTime(time: string) {
  return time.slice(0, 5)
}
