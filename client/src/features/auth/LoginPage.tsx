import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router'
import { useSession } from '@/shared/auth/session'
import { FormField } from '@/shared/ui/FormField'
import { login } from './authApi'
import { type LoginValues, loginSchema } from './schemas'

export function LoginPage() {
  const navigate = useNavigate()
  const setTokens = useSession((state) => state.setTokens)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginValues>({ resolver: zodResolver(loginSchema) })

  const loginMutation = useMutation({
    mutationFn: login,
    onSuccess: (tokens) => {
      setTokens(tokens)
      navigate('/app', { replace: true })
    },
  })

  return (
    <form
      noValidate
      onSubmit={handleSubmit((values) => loginMutation.mutate(values))}
      className="flex flex-col gap-5"
    >
      <header className="flex flex-col gap-1.5">
        <h2 className="text-3xl font-bold">Tekrar hoş geldin</h2>
        <p className="text-muted">Salonunun takvimine giriş yap.</p>
      </header>

      <FormField
        label="E-posta"
        type="email"
        autoComplete="email"
        placeholder="ornek@salonun.com"
        error={errors.email?.message}
        {...register('email')}
      />
      <FormField
        label="Şifre"
        type="password"
        autoComplete="current-password"
        error={errors.password?.message}
        {...register('password')}
      />
      <Link
        to="/sifremi-unuttum"
        className="-mt-2 self-end text-sm font-semibold text-clay hover:underline"
      >
        Şifremi unuttum
      </Link>
      <button
        type="submit"
        disabled={loginMutation.isPending}
        className="h-13 cursor-pointer rounded-full bg-clay text-[15px] font-semibold text-white transition-colors hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
      >
        {loginMutation.isPending ? 'Giriş yapılıyor…' : 'Giriş yap'}
      </button>

      <p className="text-center text-sm text-muted">
        Hesabın yok mu?{' '}
        <Link to="/register" className="font-semibold text-clay hover:underline">
          İşletmeni ücretsiz kaydet
        </Link>
      </p>
    </form>
  )
}
