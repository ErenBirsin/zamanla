import { z } from 'zod'

// sınırlar ve mesajlar backend'deki SalonServiceCreateCommandValidator ile aynı
export const serviceSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Geçerli bir hizmet adı girin')
    .max(100, 'Hizmet adı en fazla 100 karakter olabilir'),
  category: z
    .string()
    .trim()
    .min(1, 'Geçerli bir kategori girin')
    .max(100, 'Kategori en fazla 100 karakter olabilir'),
  durationMinutes: z
    .number({ error: 'Geçerli bir süre girin' })
    .int('Süre tam sayı olmalı')
    .min(5, 'Süre 5 ile 480 dakika arasında olmalı')
    .max(480, 'Süre 5 ile 480 dakika arasında olmalı'),
  bufferMinutes: z
    .number({ error: 'Geçerli bir süre girin' })
    .int('Süre tam sayı olmalı')
    .min(0, 'Hazırlık payı 0 ile 120 dakika arasında olmalı')
    .max(120, 'Hazırlık payı 0 ile 120 dakika arasında olmalı'),
  price: z.number({ error: 'Geçerli bir fiyat girin' }).min(0, 'Fiyat negatif olamaz'),
  isActive: z.boolean(),
})

export type ServiceValues = z.infer<typeof serviceSchema>
