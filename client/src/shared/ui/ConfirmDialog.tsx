import { Dialog } from './Dialog'
import { useConfirmStore } from './confirm'

// uygulamada bir kez, Toaster gibi main.tsx'te
export function ConfirmDialog() {
  const options = useConfirmStore((state) => state.options)
  const close = useConfirmStore((state) => state.close)

  return (
    <Dialog open={options !== null} title={options?.title ?? ''} onClose={close}>
      <p className="text-[15px] leading-relaxed text-ink-soft">{options?.message}</p>
      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={close}
          className="h-11 cursor-pointer rounded-full border border-line px-5 text-sm font-semibold hover:bg-sand-soft"
        >
          Vazgeç
        </button>
        <button
          type="button"
          onClick={() => {
            options?.onConfirm()
            close()
          }}
          className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark"
        >
          {options?.confirmLabel}
        </button>
      </div>
    </Dialog>
  )
}
