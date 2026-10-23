import { z } from 'zod'

// backend validator'larındaki kuralların aynısı; Türkçe harfler de harf sayılır
export const passwordRules = [
  { label: 'En az 8 karakter', test: (value: string) => value.length >= 8 },
  { label: 'Büyük harf', test: (value: string) => /\p{Lu}/u.test(value) },
  { label: 'Küçük harf', test: (value: string) => /\p{Ll}/u.test(value) },
  { label: 'Rakam', test: (value: string) => /[0-9]/.test(value) },
  { label: 'Özel karakter', test: (value: string) => /[!@#$%^&*(),.?":{}|<>]/.test(value) },
]

export const strongPasswordSchema = z
  .string()
  .min(8, 'Şifre en az 8 karakter olmalı')
  .regex(/\p{Lu}/u, 'Şifre en az bir büyük harf içermeli')
  .regex(/\p{Ll}/u, 'Şifre en az bir küçük harf içermeli')
  .regex(/[0-9]/, 'Şifre en az bir rakam içermeli')
  .regex(/[!@#$%^&*(),.?":{}|<>]/, 'Şifre en az bir özel karakter içermeli')

// hocadaki gibi: sağlanan kural sayısına göre 4 parçalı çubuk
export function getPasswordStrength(value: string) {
  const level = passwordRules.filter((rule) => rule.test(value)).length

  if (level === 0) return { level, text: 'Zayıf', tone: 'none' } as const
  if (level <= 2) return { level, text: 'Zayıf', tone: 'weak' } as const
  if (level === 3) return { level, text: 'Orta', tone: 'medium' } as const
  if (level === 4) return { level, text: 'İyi', tone: 'medium' } as const
  return { level, text: 'Güçlü', tone: 'strong' } as const
}
