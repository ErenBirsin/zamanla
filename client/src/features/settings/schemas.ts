import { z } from 'zod'
import { isValidPhone } from '@/features/customers/phone'

const rule = (min: number, max: number, message: string) =>
  z.number({ error: message }).int(message).min(min, message).max(max, message)

// sınırlar ve mesajlar backend'deki SettingsUpdateCommandValidator ile aynı
export const settingsSchema = z
  .object({
    name: z
      .string()
      .trim()
      .min(1, 'Geçerli bir salon adı girin')
      .max(100, 'Salon adı en fazla 100 karakter olabilir'),
    city: z.string().trim().max(50, 'İl en fazla 50 karakter olabilir'),
    district: z.string().trim().max(50, 'İlçe en fazla 50 karakter olabilir'),
    fullAddress: z.string().trim().max(250, 'Açık adres en fazla 250 karakter olabilir'),
    // isteğe bağlı: boşsa kontrol edilmez
    phoneNumber: z
      .string()
      .refine(
        (value) => value.trim() === '' || isValidPhone(value),
        'Geçerli bir telefon numarası girin',
      ),
    maxDaysAhead: rule(1, 180, 'Online randevu 1 ile 180 gün ilerisi arasında olabilir'),
    cancelHoursBefore: rule(0, 72, 'İptal süresi 0 ile 72 saat arasında olabilir'),
    maxUpcomingPerPhone: rule(1, 20, 'Telefon başına randevu sınırı 1 ile 20 arasında olabilir'),
  })
  // adres ya hiç girilmez ya da üç alanı da doldurulur
  .superRefine((values, ctx) => {
    const fields = ['city', 'district', 'fullAddress'] as const
    const filled = fields.filter((field) => values[field] !== '')
    if (filled.length === 0 || filled.length === fields.length) return

    const messages = { city: 'İl girin', district: 'İlçe girin', fullAddress: 'Açık adres girin' }
    for (const field of fields) {
      if (values[field] === '') {
        ctx.addIssue({ code: 'custom', path: [field], message: messages[field] })
      }
    }
  })

export type SettingsValues = z.infer<typeof settingsSchema>
