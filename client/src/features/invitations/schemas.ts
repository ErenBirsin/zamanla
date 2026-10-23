import { z } from 'zod'
import { strongPasswordSchema } from '@/shared/auth/passwordRules'

export const invitationSchema = z
  .object({
    password: strongPasswordSchema,
    passwordConfirm: z.string(),
  })
  .refine((values) => values.password === values.passwordConfirm, {
    message: 'Şifreler aynı değil',
    path: ['passwordConfirm'],
  })

export type InvitationValues = z.infer<typeof invitationSchema>
