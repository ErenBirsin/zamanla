import { useState } from 'react'
import { useParams } from 'react-router'
import { SalonHeader } from './SalonHeader'
import { SalonNotFound } from './SalonNotFound'
import { ServiceStep } from './ServiceStep'
import { usePublicSalon } from './usePublicSalon'

export function BookingPage() {
  const { slug = '' } = useParams()
  const salon = usePublicSalon(slug)
  // seçim sırası korunur: hizmetler bu sırayla arka arkaya yapılır
  const [salonServiceIds, setSalonServiceIds] = useState<string[]>([])

  if (salon.isPending) {
    return <div className="min-h-dvh bg-sand" />
  }

  if (salon.isError) {
    return <SalonNotFound error={salon.error} />
  }

  const toggleService = (id: string) => {
    setSalonServiceIds((ids) =>
      ids.includes(id) ? ids.filter((serviceId) => serviceId !== id) : [...ids, id],
    )
  }

  return (
    <div className="mx-auto flex min-h-dvh max-w-md flex-col">
      <SalonHeader salon={salon.data} />
      <main className="flex flex-col gap-4.5 px-5 pt-5.5 pb-36">
        <ServiceStep
          services={salon.data.services}
          selectedIds={salonServiceIds}
          onToggle={toggleService}
        />
      </main>
    </div>
  )
}
