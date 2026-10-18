import type { ReactNode } from 'react'

interface BookingFooterProps {
  // başlık yoksa buton bütün genişliği kaplar
  title?: string
  subtitle?: string
  children?: ReactNode
}

// telefonda başparmağın ulaştığı yerde sabit; seçimin özeti ve devam butonu
export function BookingFooter({ title, subtitle, children }: BookingFooterProps) {
  return (
    <footer className="fixed inset-x-0 bottom-0 border-t border-line bg-surface">
      <div className="mx-auto flex max-w-md items-center gap-4 px-5 pt-4 pb-7">
        {title && (
          <div className="flex min-w-0 flex-1 flex-col">
            <span className="truncate text-[15px] font-bold">{title}</span>
            <span className="truncate text-[13px] text-muted">{subtitle}</span>
          </div>
        )}
        {children}
      </div>
    </footer>
  )
}

interface ContinueButtonProps {
  disabled: boolean
  onClick: () => void
}

export function ContinueButton({ disabled, onClick }: ContinueButtonProps) {
  return (
    <button
      type="button"
      disabled={disabled}
      onClick={onClick}
      className="flex h-13 shrink-0 cursor-pointer items-center gap-2 rounded-full bg-clay px-6.5 text-[15px] font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-50"
    >
      Devam et
      <svg
        width="18"
        height="18"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2.2"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden
      >
        <path d="M5 12h14M13 6l6 6-6 6" />
      </svg>
    </button>
  )
}
