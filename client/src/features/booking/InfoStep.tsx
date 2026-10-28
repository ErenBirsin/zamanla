import { useId } from 'react'
import type { UseFormReturn } from 'react-hook-form'
import { toDateKey } from '@/features/calendar/time'
import type { AvailabilitySlot } from '@/features/calendar/types'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { formatPrice } from '@/shared/ui/format'
import { formatMonthShort, formatTimeRange } from './format'
import type { BookingValues } from './schemas'
import { StepHeading } from './StepHeading'
import { withPhoneMask } from '@/features/customers/phone'

interface InfoStepProps {
  form: UseFormReturn<BookingValues>
  onSubmit: (values: BookingValues) => void
  error: unknown
  slot: AvailabilitySlot
  endAt: string
  servicesTitle: string
  totalPrice: number
  cancelHoursBefore: number
}

export const BOOKING_FORM_ID = 'booking-form'

export function InfoStep({
  form,
  onSubmit,
  error,
  slot,
  endAt,
  servicesTitle,
  totalPrice,
  cancelHoursBefore,
}: InfoStepProps) {
  const noteId = useId()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = form
  const dateKey = toDateKey(slot.startAt)

  return (
    <section className="flex flex-col gap-5">
      <StepHeading step={3} title="Seni tanıyalım" />

      <div className="flex items-center gap-3.5 rounded-2xl bg-clay-soft p-4">
        <span className="flex size-12 shrink-0 flex-col items-center justify-center rounded-xl bg-surface leading-none">
          <span className="text-[10px] font-bold text-clay uppercase">
            {formatMonthShort(dateKey)}
          </span>
          <span className="text-xl font-bold">{Number(dateKey.slice(8))}</span>
        </span>
        <span className="flex min-w-0 flex-col gap-0.5">
          <span className="text-[15px] font-bold first-letter:uppercase">
            {formatTimeRange(slot.startAt, endAt)}
          </span>
          <span className="truncate text-[13px] text-ink-soft">
            {servicesTitle} · {slot.staffFullName} · {formatPrice(totalPrice)}
          </span>
        </span>
      </div>

      <form
        id={BOOKING_FORM_ID}
        noValidate
        onSubmit={handleSubmit(onSubmit)}
        className="flex flex-col gap-4"
      >
        <div className="grid grid-cols-2 gap-3">
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
          label="Cep telefonu"
          prefix="+90"
          type="tel"
          inputMode="tel"
          autoComplete="tel-national"
          placeholder="532 123 45 67"
          error={errors.phoneNumber?.message}
          {...withPhoneMask(register('phoneNumber'))}
        />

        <FormField
          label="E-posta"
          type="email"
          inputMode="email"
          autoComplete="email"
          placeholder="ornek@mail.com"
          hint="İsteğe bağlı. Girersen onay ve randevudan bir gün önce hatırlatma gönderilir."
          error={errors.email?.message}
          {...register('email')}
        />

        <div className="flex flex-col gap-1.5">
          <label htmlFor={noteId} className="text-[13px] font-semibold">
            {slot.staffFullName.split(' ')[0]} için not{' '}
            <span className="font-normal text-muted">(isteğe bağlı)</span>
          </label>
          <textarea
            id={noteId}
            rows={3}
            maxLength={500}
            placeholder="Örn. kısa kesim, ensede makine kullanılmasın"
            className="w-full resize-y rounded-xl border border-[#dcd0c1] bg-surface px-3.5 py-3 text-[15px] outline-none placeholder:text-muted/60 focus:border-clay focus:ring-3 focus:ring-clay-soft"
            {...register('note')}
          />
          {errors.note && <p className="text-[13px] text-clay-dark">{errors.note.message}</p>}
        </div>

        <p className="flex gap-2 text-[13px] text-muted">
          <svg
            className="mt-0.5 shrink-0"
            width="15"
            height="15"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden
          >
            <circle cx="12" cy="12" r="10" />
            <path d="M12 16v-4M12 8h.01" />
          </svg>
          {cancelHoursBefore > 0
            ? `Randevudan ${cancelHoursBefore} saat öncesine kadar ücretsiz iptal edebilirsin.`
            : 'Randevu saatine kadar ücretsiz iptal edebilirsin.'}{' '}
          Ödeme salonda yapılır.
        </p>

        <FormError error={error} />
      </form>
    </section>
  )
}
