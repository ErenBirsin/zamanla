import { z } from 'zod'

// mesajlar ve sınırlar backend'deki validator'larla aynı
export const loginSchema = z.object({
  email: z.string().trim().min(1, 'Geçerli bir mail adresi girin'),
  password: z.string().min(1, 'Geçerli bir şifre girin'),
})

export const registerSchema = z.object({
  businessName: z
    .string()
    .trim()
    .min(1, 'Geçerli bir işletme adı girin')
    .max(100, 'İşletme adı en fazla 100 karakter olabilir'),
  slug: z
    .string()
    .min(1, 'Geçerli bir kısa link girin')
    .max(100, 'Kısa link en fazla 100 karakter olabilir')
    .regex(/^[a-z0-9]+(-[a-z0-9]+)*$/, 'Kısa link yalnızca küçük harf, rakam ve tire içerebilir'),
  firstName: z
    .string()
    .trim()
    .min(1, 'Geçerli bir ad girin')
    .max(100, 'Ad en fazla 100 karakter olabilir'),
  lastName: z
    .string()
    .trim()
    .min(1, 'Geçerli bir soyad girin')
    .max(100, 'Soyad en fazla 100 karakter olabilir'),
  email: z
    .email('Geçerli bir mail adresi girin')
    .max(254, 'Mail adresi en fazla 254 karakter olabilir'),
  password: z.string().min(8, 'Şifre en az 8 karakter olmalı'),
})

export const forgotPasswordSchema = z.object({
  email: z.email('Geçerli bir mail adresi girin'),
})

export const resetPasswordSchema = z
  .object({
    newPassword: z.string().min(8, 'Şifre en az 8 karakter olmalı'),
    newPasswordConfirm: z.string(),
  })
  .refine((values) => values.newPassword === values.newPasswordConfirm, {
    message: 'Şifreler aynı değil',
    path: ['newPasswordConfirm'],
  })

export type LoginValues = z.infer<typeof loginSchema>
export type RegisterValues = z.infer<typeof registerSchema>

export type ForgotPasswordValues = z.infer<typeof forgotPasswordSchema>
export type ResetPasswordValues = z.infer<typeof resetPasswordSchema>
