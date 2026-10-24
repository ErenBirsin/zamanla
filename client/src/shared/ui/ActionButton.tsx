// tablolardaki satır düğmeleri için ikonlar (24'lük çizgi ikon)
const icons = {
  account: (
    <>
      <circle cx="8" cy="15" r="4" />
      <path d="m10.8 12.2 8.7-8.7" />
      <path d="m17 6 2.5 2.5" />
      <path d="m14.5 8.5 2 2" />
    </>
  ),
  edit: (
    <>
      <path d="M4 20h4L19 9a2.8 2.8 0 0 0-4-4L4 16v4Z" />
      <path d="m13.5 6.5 4 4" />
    </>
  ),
  delete: (
    <>
      <path d="M4 7h16" />
      <path d="M9 7V4h6v3" />
      <path d="M6 7l1 13h10l1-13" />
      <path d="M10 11v5" />
      <path d="M14 11v5" />
    </>
  ),
}

interface ActionButtonProps {
  icon: keyof typeof icons
  onClick: () => void
  children: string
  // dar ekranda yazı gizlenir, sadece ikon kalır; yazı üzerine gelince ipucu olarak görünür
  compact?: boolean
}

// sil düğmesi kırmızı tonda, diğerleri nötr
export function ActionButton({ icon, onClick, children, compact = false }: ActionButtonProps) {
  const tone =
    icon === 'delete'
      ? 'border-clay/30 text-clay-dark hover:border-clay/50 hover:bg-clay-soft'
      : 'border-line text-ink-soft hover:border-[#d6c8b6] hover:bg-sand-soft hover:text-ink'

  return (
    <button
      type="button"
      onClick={onClick}
      title={compact ? children : undefined}
      className={`inline-flex cursor-pointer items-center gap-1.5 rounded-full border bg-surface px-3 py-1.5 text-xs font-semibold transition-colors ${compact ? 'max-2xl:px-2' : ''} ${tone}`}
    >
      <svg
        width="14"
        height="14"
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
      <span className={compact ? 'sr-only 2xl:not-sr-only' : undefined}>{children}</span>
    </button>
  )
}
