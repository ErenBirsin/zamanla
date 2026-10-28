import { dayOfWeek, minutesToLabel, timeToMinutes } from '@/features/calendar/time'
import type { PublicStaff } from './types'

// o gün en erken açan ve en geç kapanan personele göre; kimse çalışmıyorsa null
export function openHours(staff: PublicStaff[], dateKey: string) {
  return openHoursOnDay(staff, dayOfWeek(dateKey))
}

// haftanın günü için (0 Pazar … 6 Cumartesi); çalışma saatleri listesinde kullanılır
export function openHoursOnDay(staff: PublicStaff[], day: number) {
  const hours = staff.flatMap((member) => member.workingHours).filter((hour) => hour.day === day)
  if (hours.length === 0) return null

  const start = Math.min(...hours.map((hour) => timeToMinutes(hour.startTime)))
  const end = Math.max(...hours.map((hour) => timeToMinutes(hour.endTime)))
  return `${minutesToLabel(start)} – ${minutesToLabel(end)}`
}

export function worksOn(member: PublicStaff, dateKey: string) {
  const day = dayOfWeek(dateKey)
  return member.workingHours.some((hour) => hour.day === day)
}

export const fullName = (member: PublicStaff) => `${member.firstName} ${member.lastName}`

export const initials = (text: string) =>
  text
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((word) => word[0].toLocaleUpperCase('tr'))
    .join('')
