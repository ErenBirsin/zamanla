import { useState } from 'react'
import { formatPrice } from '@/shared/ui/format'
import { StepHeading } from './StepHeading'
import type { PublicService } from './types'

interface ServiceStepProps {
  services: PublicService[]
  selectedIds: string[]
  onToggle: (id: string) => void
}

export function ServiceStep({ services, selectedIds, onToggle }: ServiceStepProps) {
  // liste kategoriye göre sıralı geliyor
  const categories = [...new Set(services.map((service) => service.category))]
  const [category, setCategory] = useState(categories[0])
  const visible = services.filter((service) => service.category === category)

  return (
    <section className="flex flex-col gap-4.5">
      <StepHeading step={1} title="Ne yaptırmak istersin?" />

      {services.length === 0 ? (
        <p className="rounded-2xl border border-line bg-surface p-4 text-sm text-muted">
          Bu salon şu an online randevu almıyor. Randevu için salonu arayabilirsin.
        </p>
      ) : (
        <>
          {categories.length > 1 && (
            <div className="-mx-5 flex gap-2 overflow-x-auto px-5 pb-1">
              {categories.map((item) => (
                <button
                  key={item}
                  type="button"
                  aria-pressed={item === category}
                  onClick={() => setCategory(item)}
                  className="h-9.5 shrink-0 cursor-pointer rounded-full border border-line bg-surface px-4 text-sm font-medium whitespace-nowrap text-ink-soft aria-pressed:border-ink aria-pressed:bg-ink aria-pressed:font-semibold aria-pressed:text-white"
                >
                  {item}
                </button>
              ))}
            </div>
          )}

          <div className="flex flex-col gap-2.5">
            {visible.map((service) => {
              const selected = selectedIds.includes(service.id)
              return (
                <button
                  key={service.id}
                  type="button"
                  aria-pressed={selected}
                  onClick={() => onToggle(service.id)}
                  className={`flex cursor-pointer items-center gap-3.5 rounded-2xl p-4 text-left ${selected ? 'border-2 border-clay bg-[#fcf1ec] p-[15px]' : 'border border-line bg-surface'}`}
                >
                  <span className="flex min-w-0 flex-1 flex-col gap-1">
                    <span className="text-base font-semibold">{service.name}</span>
                    <span className="text-[13px] font-semibold">
                      {service.durationMinutes} dk · {formatPrice(service.price)}
                    </span>
                  </span>
                  {selected ? (
                    <span className="flex size-6.5 shrink-0 items-center justify-center rounded-full bg-clay">
                      <svg
                        width="14"
                        height="14"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="#fff"
                        strokeWidth="3"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        aria-hidden
                      >
                        <path d="M20 6 9 17l-5-5" />
                      </svg>
                    </span>
                  ) : (
                    <span className="size-6.5 shrink-0 rounded-full border-2 border-[#cfc4b6]" />
                  )}
                </button>
              )
            })}
          </div>
          <p className="text-[13px] text-muted">
            Birden fazla hizmet seçebilirsin; arka arkaya aynı personel yapar.
          </p>
        </>
      )}
    </section>
  )
}
