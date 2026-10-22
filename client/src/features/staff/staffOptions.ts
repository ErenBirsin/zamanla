import type { StaffAccountStatus } from './types'

export const staffColors = ['peach', 'sage', 'lavender', 'sky'] as const

export type StaffColor = (typeof staffColors)[number]

export const staffColorClasses: Record<StaffColor, string> = {
  peach: 'bg-staff-peach',
  sage: 'bg-staff-sage',
  lavender: 'bg-staff-lavender',
  sky: 'bg-staff-sky',
}

export const staffColorLabels: Record<StaffColor, string> = {
  peach: 'Şeftali',
  sage: 'Adaçayı',
  lavender: 'Lavanta',
  sky: 'Gök mavisi',
}

export const weekDays = [
  { day: 1, label: 'Pazartesi', short: 'Pzt' },
  { day: 2, label: 'Salı', short: 'Sal' },
  { day: 3, label: 'Çarşamba', short: 'Çar' },
  { day: 4, label: 'Perşembe', short: 'Per' },
  { day: 5, label: 'Cuma', short: 'Cum' },
  { day: 6, label: 'Cumartesi', short: 'Cmt' },
  { day: 0, label: 'Pazar', short: 'Paz' },
]

// Personel sayfasındaki hesap rozetleri
export const accountBadgeClasses: Record<StaffAccountStatus, string> = {
  Aktif: 'bg-sage-soft text-sage',
  Pasif: 'bg-line-soft text-muted',
  'Davet gönderildi': 'bg-clay-soft text-clay-dark',
  'Davetin süresi doldu': 'bg-clay-soft text-clay-dark',
  'Hesap yok': 'border border-line text-muted',
}
