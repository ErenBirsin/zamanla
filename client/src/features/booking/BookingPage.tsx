import { useState } from 'react'
import { useParams, useSearchParams } from 'react-router'
import type { AvailabilitySlot } from '@/features/calendar/types'
import { formatPrice } from '@/shared/ui/format'
import { BookingFooter, ContinueButton } from './BookingFooter'
import { formatSlot } from './format'
import { SalonHeader } from './SalonHeader'
import { SalonNotFound } from './SalonNotFound'
import { ServiceStep } from './ServiceStep'
import { StepBar } from './StepBar'
import { TimeStep } from './TimeStep'
import { usePublicSalon } from './usePublicSalon'

export function BookingPage() {
  const { slug = '' } = useParams()
  const [searchParams, setSearchParams] = useSearchParams()
  const salon = usePublicSalon(slug)
  // seçim sırası korunur: hizmetler bu sırayla arka arkaya yapılır
  const [salonServiceIds, setSalonServiceIds] = useState<string[]>([])
  const [staffId, setStaffId] = useState('')
  const [date, setDate] = useState<string | null>(null)
  const [slot, setSlot] = useState<AvailabilitySlot | null>(null)

  if (salon.isPending) {
    return <div className="min-h-dvh bg-sand" />
  }

  if (salon.isError) {
    return <SalonNotFound error={salon.error} />
  }

  // adım adreste; telefonun geri tuşu bir önceki adıma döner
  // sayfa yenilenince seçimler sıfırlanır, gösterilebilecek en ileri adım açılır
  const maxStep = salonServiceIds.length === 0 ? 1 : 2
  const step = Math.min(Math.max(Number(searchParams.get('adim')) || 1, 1), maxStep)
  const goTo = (next: number, replace = false) =>
    setSearchParams(next === 1 ? {} : { adim: String(next) }, { replace })

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
          subtitle={`${servicesTitle} · ${servicesSummary}`}
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
        />
      )}
    </div>
  )
}
