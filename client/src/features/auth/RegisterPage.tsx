import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link, useNavigate } from 'react-router'
import { useSession } from '@/shared/auth/session'
import { FormField } from '@/shared/ui/FormField'
import { registerBusiness } from './authApi'
import { type RegisterValues, registerSchema } from './schemas'
import { slugify } from './slugify'

export function RegisterPage() {
  const navigate = useNavigate()
  const setTokens = useSession((state) => state.setTokens)
  // kullanıcı kısa linke elle dokunana kadar işletme adından üretilir
  const [slugEditedByUser, setSlugEditedByUser] = useState(false)

  const {
    register,
    handleSubmit,
    setValue,
    control,
    formState: { errors, isSubmitted },
  } = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      businessName: '',
      slug: '',
      firstName: '',
      lastName: '',
      email: '',
      password: '',
    },
  })
  const slug = useWatch({ control, name: 'slug' })

  const registerMutation = useMutation({
    mutationFn: registerBusiness,
    onSuccess: (tokens) => {
      setTokens(tokens)
      navigate('/app', { replace: true })
    },
  })

  return (
    <form
      noValidate
      onSubmit={handleSubmit((values) => registerMutation.mutate(values))}
      className="flex flex-col gap-5"
    >
      <header className="flex flex-col gap-1.5">
        <h2 className="text-3xl font-bold">İşletmeni kaydet</h2>
        <p className="text-muted">Birkaç dakikada randevu sayfan hazır olsun.</p>
      </header>

      <FormField
        label="İşletme adı"
        placeholder="Makas Studio"
        error={errors.businessName?.message}
        {...register('businessName', {
          onChange: (event) => {
            if (!slugEditedByUser) {
              setValue('slug', slugify(event.target.value), { shouldValidate: isSubmitted })
            }
          },
        })}
      />
      <FormField
        label="Kısa link"
        placeholder="makas-studio"
        error={errors.slug?.message}
        hint={`Randevu sayfan: zamanla.app/${slug || 'kisa-link'}`}
        {...register('slug', { onChange: () => setSlugEditedByUser(true) })}
      />

      <div className="grid gap-5 sm:grid-cols-2">
        <FormField
          label="Ad"
          autoComplete="given-name"
          error={errors.firstName?.message}
          {...register('firstName')}
        />
        <FormField
          label="Soyad"
          autoComplete="family-name"
          error={errors.lastName?.message}
          {...register('lastName')}
        />
      </div>

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
        autoComplete="new-password"
        hint="En az 8 karakter"
        error={errors.password?.message}
        {...register('password')}
      />

      <button
        type="submit"
        disabled={registerMutation.isPending}
        className="h-13 cursor-pointer rounded-full bg-clay text-[15px] font-semibold text-white transition-colors hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
      >
        {registerMutation.isPending ? 'Kaydediliyor…' : 'İşletmemi kaydet'}
      </button>

      <p className="text-center text-sm text-muted">
        Zaten hesabın var mı?{' '}
        <Link to="/login" className="font-semibold text-clay hover:underline">
          Giriş yap
        </Link>
      </p>
    </form>
  )
}
