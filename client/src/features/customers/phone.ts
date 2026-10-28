import type { ChangeEvent } from 'react'
import type { UseFormRegisterReturn } from 'react-hook-form'

// backend'deki PhoneNumber.Normalize ile aynı: "0532 123 45 67" → "+905321234567"
export function normalizePhone(value: string) {
  let digits = value.replace(/\D/g, '')

  if (digits.length === 12 && digits.startsWith('90')) {
    digits = digits.slice(2)
  } else if (digits.length === 11 && digits.startsWith('0')) {
    digits = digits.slice(1)
  }

  return `+90${digits}`
}

// şimdilik yalnızca Türkiye numaraları (PhoneNumber.IsValid)
export function isValidPhone(value: string) {
  return /^\+90[2-5]\d{9}$/.test(normalizePhone(value))
}

// "+905321234567" → "0532 123 45 67"
export function formatPhone(phone: string) {
  const digits = phone.replace(/^\+90/, '')
  if (digits.length !== 10) return phone
  return `0${digits.slice(0, 3)} ${digits.slice(3, 6)} ${digits.slice(6, 8)} ${digits.slice(8)}`
}

// yazarken rakamları gruplar ve fazlasını almaz: "05321234567" → "0532 123 45 67"
// +90 alanın yanında yazdığı için baştaki 0 ya da 90 alana yazılmaz
export function maskPhone(value: string) {
  let digits = value.replace(/\D/g, '')
  if (digits.startsWith('90')) digits = digits.slice(2)
  if (digits.startsWith('0')) digits = digits.slice(1)

  const national = digits.slice(0, 10)
  return [national.slice(0, 3), national.slice(3, 6), national.slice(6, 8), national.slice(8)]
    .filter(Boolean)
    .join(' ')
}

// register('phoneNumber') çıktısına maskeyi ekler; değer forma maskeli haliyle yazılır
export function withPhoneMask(field: UseFormRegisterReturn) {
  return {
    ...field,
    onChange: (event: ChangeEvent<HTMLInputElement>) => {
      event.target.value = maskPhone(event.target.value)
      return field.onChange(event)
    },
  }
}
