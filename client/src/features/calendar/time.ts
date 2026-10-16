// işletmenin saat dilimi; Türkiye'de yaz saati uygulaması yok, fark hep +03:00
export const TIME_ZONE = 'Europe/Istanbul'
const OFFSET = '+03:00'

// tarihler URL'de ve sorgu anahtarında "2026-10-19" biçiminde tutulur
export function todayKey() {
  return new Intl.DateTimeFormat('en-CA', { timeZone: TIME_ZONE }).format(new Date())
}

export function addDays(dateKey: string, days: number) {
  const [year, month, day] = dateKey.split('-').map(Number)
  return new Date(Date.UTC(year, month - 1, day + days)).toISOString().slice(0, 10)
}

// 0 = Pazar … 6 = Cumartesi, backend'deki DayOfWeek ile aynı
export function dayOfWeek(dateKey: string) {
  const [year, month, day] = dateKey.split('-').map(Number)
  return new Date(Date.UTC(year, month - 1, day)).getUTCDay()
}

// İstanbul'da 19 Ekim 00:00, UTC'de 18 Ekim 21:00 demek
export function dayRange(dateKey: string) {
  const start = new Date(`${dateKey}T00:00:00${OFFSET}`)
  const end = new Date(start.getTime() + 24 * 60 * 60 * 1000)
  return { start: start.toISOString(), end: end.toISOString() }
}

const titleFormat = new Intl.DateTimeFormat('tr-TR', {
  timeZone: TIME_ZONE,
  weekday: 'long',
  day: 'numeric',
  month: 'long',
  year: 'numeric',
})

export function formatDayTitle(dateKey: string) {
  return titleFormat.format(new Date(`${dateKey}T12:00:00${OFFSET}`))
}

const clockFormat = new Intl.DateTimeFormat('tr-TR', {
  timeZone: TIME_ZONE,
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

export function formatClock(value: string | Date) {
  return clockFormat.format(new Date(value))
}

// gece yarısından itibaren dakika, işletmenin saatine göre
export function minutesOfDay(value: string | Date) {
  const parts = clockFormat.formatToParts(new Date(value))
  const hour = Number(parts.find((part) => part.type === 'hour')!.value)
  const minute = Number(parts.find((part) => part.type === 'minute')!.value)
  return hour * 60 + minute
}

// çalışma saatleri "09:00:00" ya da OData'dan "09:00:00.0000000" gelir
export function timeToMinutes(time: string) {
  const [hour, minute] = time.split(':').map(Number)
  return hour * 60 + minute
}

export function minutesToLabel(minutes: number) {
  const hour = String(Math.floor(minutes / 60)).padStart(2, '0')
  const minute = String(minutes % 60).padStart(2, '0')
  return `${hour}:${minute}`
}

const dateTimeFormat = new Intl.DateTimeFormat('tr-TR', {
  timeZone: TIME_ZONE,
  day: 'numeric',
  month: 'long',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

// geçmiş kayıtları için: "16 Ekim 14:05"
export function formatDateTime(value: string) {
  return dateTimeFormat.format(new Date(value))
}
