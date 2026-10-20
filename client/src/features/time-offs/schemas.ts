import { z } from 'zod'
import { dayRange, toIsoString } from '@/features/calendar/time'
import type { TimeOffCreateBody } from './types'

const DAY = 24 * 60 * 60 * 1000

// sınırlar ve mesajlar backend'deki TimeOffCreateCommandValidator ile aynı
export const timeOffSchema = z
  .object({
    // boş metin bütün salon demek
    staffId: z.string(),
    allDay: z.boolean(),
    startDate: z.string().min(1, 'Gün seçin'),
    endDate: z.string(),
    startTime: z.string(),
    endTime: z.string(),
    note: z.string().trim().max(250, 'Açıklama en fazla 250 karakter olabilir'),
  })
  .superRefine((values, ctx) => {
    if (values.allDay) {
      if (values.endDate === '' || values.endDate < values.startDate) {
        ctx.addIssue({
          code: 'custom',
          path: ['endDate'],
          message: 'Son gün ilk günden önce olamaz',
        })
      } else if (Date.parse(values.endDate) - Date.parse(values.startDate) >= 90 * DAY) {
        ctx.addIssue({
          code: 'custom',
          path: ['endDate'],
          message: 'İzin en fazla 90 gün olabilir',
        })
      }
      return
    }

    if (values.startTime === '') {
      ctx.addIssue({ code: 'custom', path: ['startTime'], message: 'Başlangıç saati seçin' })
    }
    if (values.endTime === '' || values.endTime <= values.startTime) {
      ctx.addIssue({
        code: 'custom',
        path: ['endTime'],
        message: 'Bitiş, başlangıçtan sonra olmalı',
      })
    }
  })

export type TimeOffValues = z.infer<typeof timeOffSchema>

// tüm gün: ilk günün gece yarısından son günün ertesi gece yarısına
export function toTimeOffBody(values: TimeOffValues): TimeOffCreateBody {
  return {
    staffId: values.staffId || null,
    startAt: values.allDay
      ? dayRange(values.startDate).start
      : toIsoString(values.startDate, values.startTime),
    endAt: values.allDay
      ? dayRange(values.endDate).end
      : toIsoString(values.startDate, values.endTime),
    note: values.note || null,
  }
}
