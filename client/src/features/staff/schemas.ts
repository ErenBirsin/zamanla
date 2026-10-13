import { z } from 'zod'
import { staffColors } from './staffOptions'

const daySchema = z
  .object({
    day: z.number(),
    enabled: z.boolean(),
    ranges: z.array(z.object({ start: z.string(), end: z.string() })),
  })
  .superRefine((day, ctx) => {
    if (!day.enabled) return

    if (day.ranges.length === 0) {
      ctx.addIssue({ code: 'custom', message: 'En az bir saat aralığı ekleyin', path: ['ranges'] })
      return
    }

    day.ranges.forEach((range, index) => {
      if (!range.start || !range.end) {
        ctx.addIssue({
          code: 'custom',
          message: 'Başlangıç ve bitiş saatini girin',
          path: ['ranges', index, 'end'],
        })
      } else if (range.end <= range.start) {
        ctx.addIssue({
          code: 'custom',
          message: 'Bitiş saati başlangıç saatinden sonra olmalı',
          path: ['ranges', index, 'end'],
        })
      }
    })

    const sorted = [...day.ranges].sort((a, b) => a.start.localeCompare(b.start))
    if (sorted.some((range, index) => index > 0 && sorted[index - 1].end > range.start)) {
      ctx.addIssue({
        code: 'custom',
        message: 'Aynı gündeki çalışma saatleri çakışamaz',
        path: ['ranges'],
      })
    }
  })

export const staffSchema = z.object({
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
  color: z.enum(staffColors, { error: 'Geçerli bir renk seçin' }),
  salonServiceIds: z.array(z.string()),
  schedule: z.array(daySchema),
  isActive: z.boolean(),
})

export type StaffValues = z.infer<typeof staffSchema>
export type DaySchedule = StaffValues['schedule'][number]
