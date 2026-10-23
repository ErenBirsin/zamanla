import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { useForm, useWatch } from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router'
import { formatDateTime } from '@/features/calendar/time'
import { useSession } from '@/shared/auth/session'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { PasswordField } from '@/shared/ui/PasswordField'
import { PasswordStrength } from '@/shared/ui/PasswordStrength'
import { acceptInvitation, getInvitation } from './invitationsApi'
import { type InvitationValues, invitationSchema } from './schemas'

export function InvitationPage() {
  const { slug = '', token = '' } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const setTokens = useSession((state) => state.setTokens)
  // aynı tarayıcıda salon sahibi açıksa onun oturumu kapanacak
  const hasSession = useSession((state) => state.refreshToken !== null)

  const invitation = useQuery({
    queryKey: ['invitation', slug, token],
    queryFn: () => getInvitation(slug, token),
    meta: { inlineError: true },
    retry: false,
  })

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<InvitationValues>({ resolver: zodResolver(invitationSchema) })
  const password = useWatch({ control, name: 'password', defaultValue: '' })

  const acceptMutation = useMutation({
    mutationFn: (values: InvitationValues) => acceptInvitation(slug, token, values.password),
    onSuccess: (tokens) => {
      // önceki kullanıcının önbellekteki verisi yeni hesaba karışmasın
      queryClient.clear()
      setTokens(tokens)
      navigate('/app', { replace: true })
    },
    meta: { inlineError: true },
  })

  if (invitation.isPending) {
    return <p className="text-sm text-muted">Davet yükleniyor…</p>
  }

  if (invitation.isError) {
    // 404 kullanılmış ya da geri alınmış, 410 süresi dolmuş davet
    const status = isAxiosError(invitation.error) ? invitation.error.response?.status : undefined
    const known = status === 404 || status === 410

    return (
      <div className="flex flex-col gap-5">
        <header className="flex flex-col gap-1.5">
          <h2 className="text-3xl font-bold">Davet açılamadı</h2>
          {known && (
            <p className="text-muted">
              {status === 410
                ? 'Davetin süresi dolmuş. Salon sahibinden yeni bir davet isteyebilirsin.'
                : 'Bu davet kullanılmış ya da geri alınmış. Hesabın açıldıysa giriş yapabilirsin.'}
            </p>
          )}
        </header>
        {!known && <FormError error={invitation.error} />}
        <Link
          to="/login"
          className="flex h-13 items-center justify-center rounded-full border border-line bg-surface text-[15px] font-semibold hover:bg-sand-soft"
        >
          Giriş sayfasına git
        </Link>
      </div>
    )
  }

  const { salonName, staffFullName, email, expiresAt } = invitation.data

  return (
    <form
      noValidate
      onSubmit={handleSubmit((values) => acceptMutation.mutate(values))}
      className="flex flex-col gap-5"
    >
      <header className="flex flex-col gap-1.5">
        <span className="text-xs font-semibold tracking-wide text-muted">PANEL DAVETİ</span>
        <h2 className="text-3xl font-bold">{salonName}</h2>
        <p className="text-muted">
          Merhaba {staffFullName}, randevularını görmek için şifreni belirle. Davet{' '}
          {formatDateTime(expiresAt)} tarihine kadar geçerli.
        </p>
      </header>

      <FormField label="E-posta" type="email" value={email} readOnly disabled />
      <PasswordField
        label="Şifre"
        autoComplete="new-password"
        error={errors.password?.message}
        {...register('password')}
      />
      <PasswordStrength password={password} />
      <PasswordField
        label="Şifre tekrar"
        autoComplete="new-password"
        error={errors.passwordConfirm?.message}
        {...register('passwordConfirm')}
      />

      {hasSession && (
        <p className="text-sm text-muted">
          Bu tarayıcıda açık bir oturum var; hesabını açınca o oturum kapanır.
        </p>
      )}

      <FormError error={acceptMutation.error} />
      <button
        type="submit"
        disabled={acceptMutation.isPending}
        className="h-13 cursor-pointer rounded-full bg-clay text-[15px] font-semibold text-white transition-colors hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
      >
        {acceptMutation.isPending ? 'Hesap açılıyor…' : 'Hesabımı aç'}
      </button>
    </form>
  )
}
