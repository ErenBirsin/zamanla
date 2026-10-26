// veri gelene kadar sayfanın iskeleti; düz "yükleniyor" yazısı yerine sayfanın şekli görünür
export function ListSkeleton({ rows = 4 }: { rows?: number }) {
  return (
    <div
      role="status"
      aria-label="Yükleniyor"
      className="flex flex-col rounded-card border border-line bg-surface"
    >
      {Array.from({ length: rows }, (_, index) => (
        <div
          key={index}
          className="flex items-center gap-4 border-b border-line-soft px-5 py-4 last:border-b-0"
        >
          <span className="h-3.5 w-40 animate-pulse rounded-full bg-line-soft" />
          <span className="h-3.5 flex-1 animate-pulse rounded-full bg-line-soft" />
          <span className="h-3.5 w-16 animate-pulse rounded-full bg-line-soft" />
        </div>
      ))}
    </div>
  )
}

// kart ve form alanları için tek parça
export function BlockSkeleton({ className = '' }: { className?: string }) {
  return <div aria-hidden className={`animate-pulse rounded-card bg-line-soft ${className}`} />
}
