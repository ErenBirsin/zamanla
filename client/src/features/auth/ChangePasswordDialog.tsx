import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useForm, useWatch } from 'react-hook-form'
import { toast } from 'sonner'
import { useSession } from '@/shared/auth/session'
import { Dialog } from '@/shared/ui/Dialog'
import { FormError } from '@/shared/ui/FormError'
import { PasswordField } from '@/shared/ui/PasswordField'
import { PasswordStrength } from '@/shared/ui/PasswordStrength'
import { changePassword } from './authApi'
import { type ChangePasswordValues, changePasswordSchema } from './schemas'

export function ChangePasswordDialog({ onClose }: { onClose: () => void }) {
  const setTokens = useSession((state) => state.setTokens)

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<ChangePasswordValues>({ resolver: zodResolver(changePasswordSchema) })
  const newPassword = useWatch({ control, name: 'newPassword', defaultValue: '' })

  const changeMutation = useMutation({
    mutationFn: (values: ChangePasswordValues) =>
      changePassword(values.currentPassword, values.newPassword),
    onSuccess: (tokens) => {
      // eski token'lar backend'de silindi; bu cihaz yeni token'la devam eder
      setTokens(tokens)
      toast.success('Şifren değiştirildi. Diğer cihazlardaki oturumlar kapatıldı')
      onClose()
    },
    meta: { inlineError: true },
  })

  return (
    <Dialog open title="Şifreyi değiştir" onClose={onClose}>
      <form
        noValidate
        onSubmit={handleSubmit((values) => changeMutation.mutate(values))}
        className="flex flex-col gap-5"
      >
        <PasswordField
          label="Mevcut şifre"
          autoComplete="current-password"
          error={errors.currentPassword?.message}
          {...register('currentPassword')}
        />
        <PasswordField
          label="Yeni şifre"
          autoComplete="new-password"
          error={errors.newPassword?.message}
          {...register('newPassword')}
        />
        <PasswordStrength password={newPassword} />
        <PasswordField
          label="Yeni şifre tekrar"
          autoComplete="new-password"
          error={errors.newPasswordConfirm?.message}
          {...register('newPasswordConfirm')}
        />
        <p className="text-[13px] text-muted">
          Kaydedince diğer cihazlardaki oturumların kapanır; bu cihazda devam edersin.
        </p>

        <FormError error={changeMutation.error} />
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            className="h-11 cursor-pointer rounded-full border border-line px-5 text-sm font-semibold hover:bg-sand-soft"
          >
            Vazgeç
          </button>
          <button
            type="submit"
            disabled={changeMutation.isPending}
            className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
          >
            {changeMutation.isPending ? 'Kaydediliyor…' : 'Şifreyi kaydet'}
          </button>
        </div>
      </form>
    </Dialog>
  )
}
