import type { ReactNode } from 'react'

// durum sayfalarının ikonları (24'lük çizgi ikon)
const icons = {
  // kırık link: süresi dolmuş ya da kullanılmış link
  link: (
    <>
      <path d="M9 17H7A5 5 0 0 1 7 7h2" />
      <path d="M15 7h2a5 5 0 0 1 4 8" />
      <path d="M8 12h3" />
      <path d="m2 2 20 20" />
    </>
  ),
  // zarf: davet
  mail: (
    <>
      <rect x="3" y="5" width="18" height="14" rx="2" />
      <path d="m3 7 9 6 9-6" />
    </>
  ),
  // büyüteç: sayfa, salon ya da randevu bulunamadı
  search: (
    <>
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3.5-3.5" />
    </>
  ),
  // ünlem: beklenmeyen hata
  alert: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 8v4" />
      <path d="M12 16h.01" />
    </>
  ),
}

interface StatusMessageProps {
  icon: keyof typeof icons
  title: string
  message?: string
  // düğmeler ya da hata kutusu
  children?: ReactNode
}

// geçersiz link, davet açılamadı, bulunamadı gibi durumlar için ortak görünüm
export function StatusMessage({ icon, title, message, children }: StatusMessageProps) {
  return (
    <div className="flex flex-col items-center gap-5 text-center">
      <span className="flex size-14 items-center justify-center rounded-full bg-clay-soft text-clay-dark">
        <svg
          width="26"
          height="26"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden
        >
          {icons[icon]}
        </svg>
      </span>
      <div className="flex flex-col gap-1.5">
        <h1 className="text-3xl font-bold">{title}</h1>
        {message && <p className="text-muted">{message}</p>}
      </div>
      {children && <div className="flex w-full flex-col gap-3">{children}</div>}
    </div>
  )
}
