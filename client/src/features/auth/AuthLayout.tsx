import { Outlet } from 'react-router'
import { Logo } from '@/shared/ui/Logo'

const highlights = ['Online randevu sayfası', 'Otomatik hatırlatma', 'Gelmeyen müşteri takibi']

export function AuthLayout() {
  return (
    <div className="flex min-h-dvh flex-col lg:flex-row">
      <section className="flex flex-col gap-8 bg-ink px-6 py-8 text-sand sm:px-12 sm:py-10 lg:w-1/2">
        <Logo onDark />

        <div className="flex max-w-md flex-col gap-4">
          <h1 className="text-4xl leading-[1.05] font-bold sm:text-5xl">
            Telefon çalmadan dolan bir takvim.
          </h1>
          <p className="leading-relaxed text-sand/75">
            Müşterilerin linkinden randevu alır, hatırlatmalar kendiliğinden gider, çakışma olmaz.
          </p>
        </div>

        <ul className="mt-auto hidden flex-wrap gap-x-6 gap-y-2 text-sm text-sand/75 lg:flex">
          {highlights.map((item) => (
            <li key={item} className="flex items-center gap-2">
              <span className="size-1.5 rounded-full bg-clay" />
              {item}
            </li>
          ))}
        </ul>
      </section>

      <section className="flex flex-1 items-center justify-center px-6 py-10 sm:px-8">
        <div className="w-full max-w-100">
          <Outlet />
        </div>
      </section>
    </div>
  )
}
