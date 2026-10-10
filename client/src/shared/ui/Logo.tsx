interface LogoProps {
  onDark?: boolean
}

export function Logo({ onDark = false }: LogoProps) {
  return (
    <div className="flex items-center gap-2.5">
      <span className="flex size-9 items-center justify-center rounded-[11px] bg-clay">
        <svg
          width="20"
          height="20"
          viewBox="0 0 24 24"
          fill="none"
          stroke="#fff"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden="true"
        >
          <circle cx="12" cy="13" r="8" />
          <path d="M12 9v4l2.5 2" />
          <path d="M9 2h6" />
        </svg>
      </span>
      <span
        className={`font-display text-[22px] font-bold tracking-tight ${onDark ? 'text-sand' : 'text-ink'}`}
      >
        zamanla
      </span>
    </div>
  )
}
