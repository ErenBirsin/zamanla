import { type ReactNode, useEffect, useRef } from 'react'

interface DialogProps {
  open: boolean
  title: string
  onClose: () => void
  children: ReactNode
}

// tarayıcının kendi <dialog> öğesi: Esc ile kapanma ve odak yönetimi hazır gelir
export function Dialog({ open, title, onClose, children }: DialogProps) {
  const ref = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  return (
    <dialog
      ref={ref}
      onClose={onClose}
      aria-labelledby="dialog-title"
      className="m-auto max-h-[calc(100dvh-2rem)] w-[calc(100%-2rem)] max-w-lg overflow-hidden rounded-card bg-surface p-0 text-ink shadow-xl backdrop:bg-ink/40"
    >
      {/* uzun formlarda pencere ekrana sığar, sadece içi kayar; başlık üstte sabit kalır */}
      <div className="max-h-[calc(100dvh-2rem)] [scrollbar-width:thin] [scrollbar-color:var(--color-line)_transparent] overflow-y-auto">
        <h2
          id="dialog-title"
          className="sticky top-0 z-10 bg-surface px-6 pt-6 pb-5 text-2xl font-bold"
        >
          {title}
        </h2>
        <div className="flex flex-col gap-5 px-6 pb-6">{children}</div>
      </div>
    </dialog>
  )
}
