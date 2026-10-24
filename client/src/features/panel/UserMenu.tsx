import { useEffect, useRef, useState } from 'react'

interface UserMenuProps {
  fullName: string
  roleLabel: string
  email: string
  onChangePassword: () => void
  onLogout: () => void
}

// sol menünün altındaki kullanıcı kartı; tıklayınca hesap işlemleri açılır
export function UserMenu({
  fullName,
  roleLabel,
  email,
  onChangePassword,
  onLogout,
}: UserMenuProps) {
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  // dışarı tıklayınca ya da Esc ile kapanır
  useEffect(() => {
    if (!open) return

    const onPointerDown = (event: PointerEvent) => {
      if (!containerRef.current?.contains(event.target as Node)) setOpen(false)
    }
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false)
    }
    document.addEventListener('pointerdown', onPointerDown)
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('pointerdown', onPointerDown)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [open])

  const initials = fullName
    .split(' ')
    .map((part) => part[0])
    .join('')
    .slice(0, 2)
    .toLocaleUpperCase('tr')

  // menü kapanır, sonra seçilen işlem çalışır
  const choose = (action: () => void) => {
    setOpen(false)
    action()
  }

  return (
    <div ref={containerRef} className="relative">
      {open && (
        <div className="absolute inset-x-0 bottom-full z-20 mb-2 flex flex-col rounded-card border border-line bg-surface py-1.5 shadow-xl">
          <span className="truncate border-b border-line-soft px-4 pt-1.5 pb-2.5 text-xs text-muted">
            {email}
          </span>
          <button
            type="button"
            onClick={() => choose(onChangePassword)}
            className="flex cursor-pointer items-center gap-2.5 px-4 py-2.5 text-left text-sm font-semibold text-ink-soft hover:bg-sand-soft"
          >
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
              <rect x="4" y="11" width="16" height="10" rx="2" />
              <path d="M8 11V7a4 4 0 0 1 8 0v4" />
            </svg>
            Şifreyi değiştir
          </button>
          <button
            type="button"
            onClick={() => choose(onLogout)}
            className="flex cursor-pointer items-center gap-2.5 px-4 py-2.5 text-left text-sm font-semibold text-clay-dark hover:bg-clay-soft"
          >
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
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
              <path d="m16 17 5-5-5-5" />
              <path d="M21 12H9" />
            </svg>
            Çıkış yap
          </button>
        </div>
      )}

      <button
        type="button"
        aria-expanded={open}
        aria-haspopup="menu"
        onClick={() => setOpen((value) => !value)}
        className="flex w-full cursor-pointer items-center gap-3 rounded-xl border border-line bg-surface px-3 py-2.5 text-left hover:bg-sand-soft"
      >
        <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-clay-soft text-xs font-bold text-clay-dark">
          {initials}
        </span>
        <span className="flex min-w-0 flex-1 flex-col">
          <span className="truncate text-sm font-semibold">{fullName}</span>
          <span className="truncate text-xs text-muted">{roleLabel}</span>
        </span>
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
          className={`shrink-0 text-muted transition-transform ${open ? 'rotate-180' : ''}`}
        >
          <path d="m18 15-6-6-6 6" />
        </svg>
      </button>
    </div>
  )
}
