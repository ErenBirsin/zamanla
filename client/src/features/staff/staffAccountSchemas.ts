import { z } from 'zod'
import type { Permission } from '@/shared/auth/permissions'

// mesaj ve sınır backend'deki InvitationCreateCommandValidator ile aynı
export const inviteSchema = z.object({
  email: z
    .email('Geçerli bir mail adresi girin')
    .max(254, 'Mail adresi en fazla 254 karakter olabilir'),
  permissions: z.array(z.custom<Permission>()),
})

export type InviteValues = z.infer<typeof inviteSchema>
