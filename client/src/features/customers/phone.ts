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
