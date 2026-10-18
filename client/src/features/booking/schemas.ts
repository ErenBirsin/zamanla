import type { z } from 'zod'
import { customerSchema } from '@/features/customers/schemas'

export const bookingSchema = customerSchema.pick({
  firstName: true,
  lastName: true,
  phoneNumber: true,
  note: true,
})

export type BookingValues = z.infer<typeof bookingSchema>
