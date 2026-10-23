import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { useForm, useWatch } from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router'
import { toast } from 'sonner'
import { useSession } from '@/shared/auth/session'
import { FormError } from '@/shared/ui/FormError'
import { PasswordField } from '@/shared/ui/PasswordField'
import { PasswordStrength } from '@/shared/ui/PasswordStrength'
import { checkForgotPasswordCode, resetPassword } from './authApi'
import { type ResetPasswordValues, resetPasswordSchema } from './schemas'

export function ResetPasswordPage() {
  const { code = '' } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const clearSession = useSession((state) => state.clear)
  // aynı tarayıcıda açık bir oturum varsa kapanacak
  const hasSession = useSession((state) => state.refreshToken !== null)

  const check = useQuery({
    queryKey: ['forgot-password-code', code],
    queryFn: () => checkForgotPasswordCode(code),
    meta: { inlineError: true },
    retry: false,
  })

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<ResetPasswordValues>({ resolver: zodResolver(resetPasswordSchema) })
  const newPassword = useWatch({ control, name: 'newPassword', defaultValue: '' })

  const resetMutation = useMutation({
    mutationFn: (values: ResetPasswordValues) => resetPassword(code, values.newPassword),
    onSuccess: (message) => {
      // backend bu hesabın oturumlarını kapattı; tarayıcıdaki oturum ve önbellek de temizlenir
      queryClient.clear()
      clearSession()
      toast.success(message)
      navigate('/login', { replace: true })
    },
    meta: { inlineError: true },
  })

  if (check.isPending) {
    return <p className="text-sm text-muted">Link kontrol ediliyor…</p>
  }

  if (check.isError) {
    // 404: link yanlış, kullanılmış ya da süresi dolmuş
    const invalid = isAxiosError(check.error) && check.error.response?.status === 404

    return (
      <div className="flex flex-col gap-5">
        <header className="flex flex-col gap-1.5">
          <h2 className="text-3xl font-bold">Link açılamadı</h2>
          {invalid && (
            <p className="text-muted">
              Bu link geçersiz, kullanılmış ya da süresi dolmuş. Yeni bir link isteyebilirsin.
            </p>
          )}
        </header>
        {!invalid && <FormError error={check.error} />}
        <Link
          to="/sifremi-unuttum"
          className="flex h-13 items-center justify-center rounded-full bg-clay text-[15px] font-semibold text-white hover:bg-clay-dark"
        >
          Yeni link iste
        </Link>
        <Link to="/login" className="text-center text-sm font-semibold text-clay hover:underline">
          Giriş sayfasına dön
        </Link>
      </div>
    )
  }

  return (
    <form
      noValidate
      onSubmit={handleSubmit((values) => resetMutation.mutate(values))}
      className="flex flex-col gap-5"
    >
      <header className="flex flex-col gap-1.5">
        <h2 className="text-3xl font-bold">Yeni şifreni belirle</h2>
        <p className="text-muted">Kaydettikten sonra yeni şifrenle giriş yapabilirsin.</p>
      </header>

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

      {hasSession && (
        <p className="text-sm text-muted">
          Bu tarayıcıda açık bir oturum var; şifreni değiştirince o oturum kapanır.
        </p>
      )}

      <FormError error={resetMutation.error} />
      <button
        type="submit"
        disabled={resetMutation.isPending}
        className="h-13 cursor-pointer rounded-full bg-clay text-[15px] font-semibold text-white transition-colors hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
      >
        {resetMutation.isPending ? 'Kaydediliyor…' : 'Şifremi kaydet'}
      </button>
    </form>
  )
}
