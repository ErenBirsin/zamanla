import { Outlet } from 'react-router'
import { Logo } from '@/shared/ui/Logo'

// sol paneldeki örnek takvim; gerçek veri değil, ürünün nasıl göründüğünü anlatır
const previewAppointments = [
  {
    time: '10:00',
    name: 'Ayşe Y.',
    service: 'Saç kesimi',
    staff: 'Elif',
    tone: 'bg-staff-peach border-staff-peach-strong',
  },
  {
    time: '11:30',
    name: 'Can A.',
    service: 'Sakal tıraşı',
    staff: 'Mert',
    tone: 'bg-staff-sage border-staff-sage-strong',
  },
  {
    time: '14:00',
    name: 'Selin Ç.',
    service: 'Saç boyama',
    staff: 'Zeynep',
    tone: 'bg-staff-sky border-staff-sky-strong',
  },
]

const highlights = [
  {
    title: 'Online randevu sayfası',
    text: 'Müşterin linkinden hizmet, personel ve saat seçer.',
    icon: 'link',
  },
  {
    title: 'Otomatik hatırlatma',
    text: 'Randevudan 24 saat önce e-posta ve SMS gider.',
    icon: 'bell',
  },
  {
    title: 'Çakışma olmaz',
    text: 'Boş saatler mesai, mola ve izinlere göre hesaplanır.',
    icon: 'check',
  },
] as const

const icons = {
  link: (
    <>
      <path d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7" />
      <path d="M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7" />
    </>
  ),
  bell: (
    <>
      <path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9" />
      <path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" />
    </>
  ),
  check: <path d="M20 6 9 17l-5-5" />,
}

function ProductPreview() {
  return (
    <div aria-hidden className="relative max-w-md">
      <div className="rounded-card bg-surface p-4 text-ink shadow-2xl shadow-black/30">
        <div className="mb-3 flex items-center justify-between px-1">
          <span className="text-sm font-bold">Bugün</span>
          <span className="rounded-full bg-sand px-2.5 py-1 text-xs font-semibold text-ink-soft">
            3 randevu
          </span>
        </div>
        <ul className="flex flex-col gap-2">
          {previewAppointments.map((item) => (
            <li
              key={item.time}
              className={`flex items-center gap-3 rounded-lg border-l-[3px] px-3 py-2.5 ${item.tone}`}
            >
              <span className="w-11 text-xs font-semibold text-ink-soft tabular-nums">
                {item.time}
              </span>
              <span className="flex min-w-0 flex-1 flex-col">
                <span className="truncate text-sm font-semibold">{item.name}</span>
                <span className="truncate text-xs text-ink-soft">{item.service}</span>
              </span>
              <span className="text-xs text-muted">{item.staff}</span>
            </li>
          ))}
        </ul>
      </div>

      {/* online randevu bildirimi */}
      <div className="absolute -right-4 -bottom-5 flex items-center gap-2.5 rounded-full bg-surface py-2 pr-4 pl-2 text-ink shadow-xl shadow-black/30">
        <span className="flex size-7 items-center justify-center rounded-full bg-clay text-white">
          <svg
            width="14"
            height="14"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="M20 6 9 17l-5-5" />
          </svg>
        </span>
        <span className="text-xs font-semibold">Yeni online randevu · 16:30</span>
      </div>
    </div>
  )
}

export function AuthLayout() {
  return (
    <div className="flex min-h-dvh flex-col lg:flex-row">
      {/* telefonda sadece logo; tanıtım bilgisayarda */}
      <section className="flex flex-col bg-ink px-6 py-5 text-sand sm:px-12 lg:w-1/2 lg:gap-10 lg:py-10">
        <Logo onDark />

        {/* geniş ekranda içerik panelin ortasında durur, yazılar okunur genişlikte kalır */}
        <div className="mx-auto my-auto hidden w-full max-w-xl flex-col gap-12 lg:flex">
          <div className="flex max-w-md flex-col gap-4">
            <h1 className="text-5xl leading-[1.05] font-bold">
              Telefon çalmadan dolan bir takvim.
            </h1>
            <p className="leading-relaxed text-sand/75">
              Müşterilerin linkinden randevu alır, hatırlatmalar kendiliğinden gider, çakışma olmaz.
            </p>
          </div>
          <ProductPreview />

          <ul className="grid grid-cols-3 gap-6">
            {highlights.map((item) => (
              <li key={item.title} className="flex flex-col gap-2">
                <span className="flex size-8 items-center justify-center rounded-full bg-clay/20 text-clay">
                  <svg
                    width="16"
                    height="16"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    aria-hidden
                  >
                    {icons[item.icon]}
                  </svg>
                </span>
                <span className="text-sm font-semibold">{item.title}</span>
                <span className="text-xs leading-relaxed text-sand/65">{item.text}</span>
              </li>
            ))}
          </ul>
        </div>
      </section>

      <section className="flex flex-1 items-start justify-center px-6 py-10 sm:items-center sm:px-8">
        <div className="w-full max-w-100">
          <Outlet />
        </div>
      </section>
    </div>
  )
}
