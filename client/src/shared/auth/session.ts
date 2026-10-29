import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { AuthTokens } from '@/shared/models/auth'

interface SessionState {
  accessToken: string | null
  refreshToken: string | null
  setTokens: (tokens: AuthTokens) => void
  clear: () => void
}

export const useSession = create<SessionState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      setTokens: ({ accessToken, refreshToken }) => set({ accessToken, refreshToken }),
      clear: () => set({ accessToken: null, refreshToken: null }),
    }),
    { name: 'zamanla-session' },
  ),
)

// başka sekmede giriş, çıkış ya da token yenileme olunca bu sekme de aynı oturuma geçer
window.addEventListener('storage', (event) => {
  if (event.key === 'zamanla-session') {
    void useSession.persist.rehydrate()
  }
})
