import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { forgotPassword } from './authApi'
import { type ForgotPasswordValues, forgotPasswordSchema } from './schemas'

export function ForgotPasswordPage() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordValues>({ resolver: zodResolver(forgotPasswordSchema) })

  const forgotMutation = useMutation({
    mutationFn: forgotPassword,
    meta: { inlineError: true },
  })

  // cevap hesap olsun olmasın aynı; kayıtlı e-postalar belli olmasın
  if (forgotMutation.isSuccess) {
    return (
      <div className="flex flex-col gap-5">
        <header className="flex flex-col gap-1.5">
          <h2 className="text-3xl font-bold">Gelen kutunu kontrol et</h2>
          <p className="text-muted">{forgotMutation.data}</p>
          <p className="text-sm text-muted">
            Link 1 saat geçerli. Mail gelmediyse spam klasörüne bak ya da adresi kontrol edip tekrar
            dene.
          </p>
        </header>
        <button
          type="button"
          onClick={() => forgotMutation.reset()}
          className="h-13 cursor-pointer rounded-full border border-line bg-surface text-[15px] font-semibold hover:bg-sand-soft"
        >
          Başka bir adres dene
        </button>
        <Link to="/login" className="text-center text-sm font-semibold text-clay hover:underline">
          Giriş sayfasına dön
        </Link>
      </div>
    )
  }

  return (
    <form
      noValidate
      onSubmit={handleSubmit((values) => forgotMutation.mutate(values))}
      className="flex flex-col gap-5"
    >
      <header className="flex flex-col gap-1.5">
        <h2 className="text-3xl font-bold">Şifreni mi unuttun?</h2>
        <p className="text-muted">
          Hesabının mail adresini yaz, şifreni yenilemen için bir link gönderelim.
        </p>
      </header>

      <FormField
        label="E-posta"
        type="email"
        autoComplete="email"
        placeholder="ornek@salonun.com"
        error={errors.email?.message}
        {...register('email')}
      />

      <FormError error={forgotMutation.error} />
      <button
        type="submit"
        disabled={forgotMutation.isPending}
        className="h-13 cursor-pointer rounded-full bg-clay text-[15px] font-semibold text-white transition-colors hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
      >
        {forgotMutation.isPending ? 'Gönderiliyor…' : 'Link gönder'}
      </button>

      <p className="text-center text-sm text-muted">
        Şifreni hatırladın mı?{' '}
        <Link to="/login" className="font-semibold text-clay hover:underline">
          Giriş yap
        </Link>
      </p>
    </form>
  )
}
