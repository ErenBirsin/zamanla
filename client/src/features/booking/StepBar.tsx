interface StepBarProps {
  salonName: string
  subtitle: string
  onBack: () => void
}

// 2. ve 3. adımda koyu başlığın yerine; geri ok bir önceki adıma döner
export function StepBar({ salonName, subtitle, onBack }: StepBarProps) {
  return (
    <header className="flex items-center gap-3.5 px-5 pt-5">
      <button
        type="button"
        aria-label="Geri"
        onClick={onBack}
        className="flex size-11 shrink-0 cursor-pointer items-center justify-center rounded-full border border-line bg-surface hover:bg-sand-soft"
      >
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
          <path d="M19 12H5M11 18l-6-6 6-6" />
        </svg>
      </button>
      <div className="flex min-w-0 flex-col">
        <span className="truncate text-[15px] font-bold">{salonName}</span>
        <span className="truncate text-xs text-muted">{subtitle}</span>
      </div>
    </header>
  )
}
