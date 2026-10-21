import { TIME_ZONE } from '@/features/calendar/time'

const shortDayFormat = new Intl.DateTimeFormat('tr-TR', {
  timeZone: TIME_ZONE,
  weekday: 'short',
  day: 'numeric',
})

// grafiğin alt satırı: "21 Çar"
export function formatShortDay(dateKey: string) {
  return shortDayFormat.format(new Date(`${dateKey}T12:00:00+03:00`))
}

// eksende kuruş gösterilmez: "₺6.900"
const wholeCurrency = new Intl.NumberFormat('tr-TR', {
  style: 'currency',
  currency: 'TRY',
  maximumFractionDigits: 0,
})

export function formatWholePrice(value: number) {
  return wholeCurrency.format(value)
}
