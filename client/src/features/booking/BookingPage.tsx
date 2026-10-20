import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate, useParams, useSearchParams } from 'react-router'
import { toast } from 'sonner'
import type { AvailabilitySlot } from '@/features/calendar/types'
import { getErrorMessages } from '@/shared/api/http'
import { formatPrice } from '@/shared/ui/format'
import { BookingFooter, ContinueButton } from './BookingFooter'
import { createPublicAppointment } from './bookingApi'
import { formatSlot } from './format'
import { BOOKING_FORM_ID, InfoStep } from './InfoStep'
import { SalonHeader } from './SalonHeader'
import { SalonNotFound } from './SalonNotFound'
import { type BookingValues, bookingSchema } from './schemas'
import { ServiceStep } from './ServiceStep'
import { StepBar } from './StepBar'
import { TimeStep } from './TimeStep'
import { publicQueryKey, usePublicSalon } from './usePublicSalon'

export function BookingPage() {
  const { slug = '' } = useParams()
  const [searchParams, setSearchParams] = useSearchParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const salon = usePublicSalon(slug)
  // seçim sırası korunur: hizmetler bu sırayla arka arkaya yapılır
  const [salonServiceIds, setSalonServiceIds] = useState<string[]>([])
  const [staffId, setStaffId] = useState('')
  const [date, setDate] = useState<string | null>(null)
  const [slot, setSlot] = useState<AvailabilitySlot | null>(null)
  // form sayfada tutulur; saat değiştirmek için geri dönülse de yazılanlar kaybolmaz
  const form = useForm<BookingValues>({
    resolver: zodResolver(bookingSchema),
    defaultValues: { firstName: '', lastName: '', phoneNumber: '', email: '', note: '' },
  })

  const goTo = (next: number, replace = false) =>
    setSearchParams(next === 1 ? {} : { adim: String(next) }, { replace })

  const bookMutation = useMutation({
    mutationFn: (values: BookingValues) =>
      createPublicAppointment(slug, {
        ...values,
        email: values.email || null,
        staffId: slot!.staffId,
        salonServiceIds,
        startAt: slot!.startAt,
      }),
    onSuccess: (created) => {
      void queryClient.invalidateQueries({ queryKey: publicQueryKey(slug) })
      // onay ekranı yönetim sayfasının kendisi; adresi kaydeden randevusuna her zaman döner
      navigate(`/b/${slug}/r/${created.manageToken}`, {
        replace: true,
        state: { justBooked: true },
      })
    },
    onError: (error) => {
      // saat bu arada dolduysa boş saatler yenilenir, müşteri başka saat seçer
      if (isAxiosError(error) && error.response?.status === 409) {
        getErrorMessages(error).forEach((message) => toast.error(message))
        setSlot(null)
        void queryClient.invalidateQueries({ queryKey: publicQueryKey(slug) })
        goTo(2, true)
      }
    },
    meta: { inlineError: true },
  })

  if (salon.isPending) {
    return <div className="min-h-dvh bg-sand" />
  }

  if (salon.isError) {
    return <SalonNotFound error={salon.error} />
  }

  // adım adreste; telefonun geri tuşu bir önceki adıma döner
  // sayfa yenilenince seçimler sıfırlanır, gösterilebilecek en ileri adım açılır
  const maxStep = salonServiceIds.length === 0 ? 1 : slot ? 3 : 2
  const step = Math.min(Math.max(Number(searchParams.get('adim')) || 1, 1), maxStep)

  const selectedServices = salonServiceIds
    .map((id) => salon.data.services.find((service) => service.id === id))
    .filter((service) => service !== undefined)
  const totalDuration = selectedServices.reduce((sum, service) => sum + service.durationMinutes, 0)
  const totalPrice = selectedServices.reduce((sum, service) => sum + service.price, 0)
  const servicesTitle = selectedServices.map((service) => service.name).join(' + ')
  const servicesSummary = `${totalDuration} dk · ${formatPrice(totalPrice)}`

  const toggleService = (id: string) => {
    setSalonServiceIds((ids) =>
      ids.includes(id) ? ids.filter((serviceId) => serviceId !== id) : [...ids, id],
    )
    setSlot(null)
  }

  return (
    <div className="mx-auto flex min-h-dvh max-w-md flex-col">
      {step === 1 ? (
        <SalonHeader salon={salon.data} />
      ) : (
        <StepBar
          salonName={salon.data.name}
          subtitle={step === 2 ? `${servicesTitle} · ${servicesSummary}` : 'Son adım'}
          onBack={() => goTo(step - 1, true)}
        />
      )}

      <main className="flex flex-col gap-4.5 px-5 pt-5.5 pb-36">
        {step === 1 && (
          <ServiceStep
            services={salon.data.services}
            selectedIds={salonServiceIds}
            onToggle={toggleService}
          />
        )}
        {step === 2 && (
          <TimeStep
            slug={slug}
            salon={salon.data}
            salonServiceIds={salonServiceIds}
            staffId={staffId}
            onStaffChange={(id) => {
              setStaffId(id)
              setSlot(null)
            }}
            date={date}
            onDateChange={(day) => {
              setDate(day)
              setSlot(null)
            }}
            slot={slot}
            onSlotChange={setSlot}
          />
        )}
        {step === 3 && slot && (
          <InfoStep
            form={form}
            onSubmit={(values) => bookMutation.mutate(values)}
            error={bookMutation.error}
            slot={slot}
            endAt={new Date(
              new Date(slot.startAt).getTime() + totalDuration * 60_000,
            ).toISOString()}
            servicesTitle={servicesTitle}
            totalPrice={totalPrice}
            cancelHoursBefore={salon.data.cancelHoursBefore}
          />
        )}
      </main>

      {step === 1 && (
        <BookingFooter
          title={servicesTitle || 'Hizmet seç'}
          subtitle={selectedServices.length > 0 ? servicesSummary : 'Birden fazla seçebilirsin'}
        >
          <ContinueButton disabled={selectedServices.length === 0} onClick={() => goTo(2)} />
        </BookingFooter>
      )}
      {step === 2 && (
        <BookingFooter
          title={slot ? formatSlot(slot.startAt) : 'Saat seç'}
          subtitle={slot ? `${slot.staffFullName} ile, ${totalDuration} dk` : servicesSummary}
        >
          <ContinueButton disabled={!slot} onClick={() => goTo(3)} />
        </BookingFooter>
      )}
      {step === 3 && (
        <BookingFooter>
          <button
            type="submit"
            form={BOOKING_FORM_ID}
            disabled={bookMutation.isPending}
            className="h-13 flex-1 cursor-pointer rounded-full bg-clay text-base font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
          >
            {bookMutation.isPending ? 'Kaydediliyor…' : 'Randevuyu onayla'}
          </button>
        </BookingFooter>
      )}
    </div>
  )
}
