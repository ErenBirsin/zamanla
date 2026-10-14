import { z } from 'zod'
import { isValidPhone } from './phone'

export const customerSchema = z.object({
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
  phoneNumber: z.string().refine(isValidPhone, 'Geçerli bir telefon numarası girin'),
  // isteğe bağlı: boşsa kontrol edilmez, backend null olarak kaydeder
  email: z
    .string()
    .trim()
    .max(254, 'Mail adresi en fazla 254 karakter olabilir')
    // Türkçe klavyede sık yapılan hata; e-posta sağlayıcıları bu harfleri kabul etmiyor
    .refine((value) => !/[çğıöşüÇĞİÖŞÜ]/.test(value), {
      message: 'E-posta adresinde Türkçe karakter (ç, ğ, ı, ö, ş, ü) kullanılamaz',
    })
    .refine((value) => value === '' || z.email().safeParse(value).success, {
      message: 'Geçerli bir mail adresi girin',
    }),
  note: z.string().trim().max(500, 'Not en fazla 500 karakter olabilir'),
})

export type CustomerValues = z.infer<typeof customerSchema>
