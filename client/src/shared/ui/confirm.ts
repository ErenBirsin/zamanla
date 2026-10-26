import { create } from 'zustand'

interface ConfirmOptions {
  title: string
  message: string
  confirmLabel: string
  onConfirm: () => void
}

interface ConfirmState {
  options: ConfirmOptions | null
  open: (options: ConfirmOptions) => void
  close: () => void
}

export const useConfirmStore = create<ConfirmState>()((set) => ({
  options: null,
  open: (options) => set({ options }),
  close: () => set({ options: null }),
}))

// hocanın showSwal(başlık, metin, düğme, callback) karşılığı; her yerden çağrılır, pencere tek
export function confirmAction(options: ConfirmOptions) {
  useConfirmStore.getState().open(options)
}
