import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { formatPhone } from '@/features/customers/phone'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { type SettingsValues, settingsSchema } from './schemas'
import { updateSettings } from './settingsApi'
import type { Settings } from './types'
import { settingsQueryKey, useSettings } from './useSettings'

function toFormValues(settings: Settings): SettingsValues {
  return {
    name: settings.name,
    city: settings.address?.city ?? '',
    district: settings.address?.district ?? '',
    fullAddress: settings.address?.fullAddress ?? '',
    phoneNumber: settings.phoneNumber ? formatPhone(settings.phoneNumber) : '',
    ...settings.bookingPolicy,
  }
}

const section = 'flex flex-col gap-4 rounded-card border border-line bg-surface p-6'

export function SettingsPage() {
  const { data: settings, isPending } = useSettings()
  const queryClient = useQueryClient()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<SettingsValues>({ resolver: zodResolver(settingsSchema) })

  // kayıttan sonra gelen yeni değerler formun başlangıcı olur, "Kaydet" yeniden pasifleşir
  useEffect(() => {
    if (settings) reset(toFormValues(settings))
  }, [settings, reset])

  const saveMutation = useMutation({
    mutationFn: updateSettings,
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: settingsQueryKey })
      // yan menüdeki salon adı ve randevu sayfası da yenilensin
      void queryClient.invalidateQueries({ queryKey: ['current-user'] })
      void queryClient.invalidateQueries({ queryKey: ['public'] })
    },
    meta: { inlineError: true },
  })

  return (
    <>
      <header className="flex flex-col gap-1.5">
        <h1 className="text-3xl font-bold">Ayarlar</h1>
        <p className="text-muted">Salon bilgilerin ve online randevu kuralların.</p>
      </header>

      {isPending ? (
        <p className="text-sm text-muted">Ayarlar yükleniyor…</p>
      ) : (
        <form
          noValidate
          onSubmit={handleSubmit((values) => saveMutation.mutate(values))}
          className="flex max-w-2xl flex-col gap-5"
        >
          <section className={section}>
            <h2 className="text-lg font-bold">Salon bilgileri</h2>
            <FormField label="Salon adı" error={errors.name?.message} {...register('name')} />
            <div className="flex flex-col gap-1.5">
              <span className="text-[13px] font-semibold">Randevu linki</span>
              <code className="rounded-xl border border-line-soft bg-sand-soft px-3.5 py-3 text-sm">
                {window.location.host}/b/{settings?.slug}
              </code>
              <p className="text-[13px] text-muted">
                Link değiştirilemez; müşterilerle paylaştığın linkler bozulmasın diye.
              </p>
            </div>
            <FormField
              label="Telefon"
              type="tel"
              inputMode="tel"
              autoComplete="off"
              placeholder="0216 123 45 67"
              hint="İsteğe bağlı; randevu sayfasında müşterinin arayabileceği numara."
              error={errors.phoneNumber?.message}
              {...register('phoneNumber')}
            />
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField label="İl" error={errors.city?.message} {...register('city')} />
              <FormField label="İlçe" error={errors.district?.message} {...register('district')} />
            </div>
            <FormField
              label="Açık adres"
              hint="İsteğe bağlı; girersen il ve ilçe de gerekir."
              error={errors.fullAddress?.message}
              {...register('fullAddress')}
            />
          </section>

          <section className={section}>
            <h2 className="text-lg font-bold">Online randevu kuralları</h2>
            <FormField
              label="En fazla kaç gün ilerisi"
              type="number"
              inputMode="numeric"
              hint="Müşteri randevu sayfasında bugünden en fazla bu kadar gün sonrasını seçebilir."
              error={errors.maxDaysAhead?.message}
              {...register('maxDaysAhead', { valueAsNumber: true })}
            />
            <FormField
              label="Linkten iptal için son süre (saat)"
              type="number"
              inputMode="numeric"
              hint="Randevuya bu kadar saat kala müşteri linkten iptal edemez; 0 girersen son ana kadar edebilir."
              error={errors.cancelHoursBefore?.message}
              {...register('cancelHoursBefore', { valueAsNumber: true })}
            />
            <FormField
              label="Aynı telefonla en fazla ileri tarihli randevu"
              type="number"
              inputMode="numeric"
              hint="Takvimin sahte randevularla dolmaması için."
              error={errors.maxUpcomingPerPhone?.message}
              {...register('maxUpcomingPerPhone', { valueAsNumber: true })}
            />
          </section>

          <FormError error={saveMutation.error} />
          <div className="flex justify-end">
            <button
              type="submit"
              disabled={!isDirty || saveMutation.isPending}
              className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
            >
              {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
            </button>
          </div>
        </form>
      )}
    </>
  )
}
