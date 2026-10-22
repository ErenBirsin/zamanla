import { z } from 'zod'

export const invitationSchema = z
  .object({
    password: z.string().min(8, 'Şifre en az 8 karakter olmalı'),
    passwordConfirm: z.string(),
  })
  .refine((values) => values.password === values.passwordConfirm, {
    message: 'Şifreler aynı değil',
    path: ['passwordConfirm'],
  })

export type InvitationValues = z.infer<typeof invitationSchema>
