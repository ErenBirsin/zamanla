import { QueryClientProvider } from '@tanstack/react-query'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router'
import { Toaster } from 'sonner'
import '@fontsource-variable/bricolage-grotesque'
import '@fontsource-variable/plus-jakarta-sans'
import './index.css'
import { queryClient } from './shared/api/queryClient'
import { router } from './router'
import { ConfirmDialog } from './shared/ui/ConfirmDialog'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
      <Toaster position="top-center" richColors style={{ fontFamily: 'var(--font-sans)' }} />
      <ConfirmDialog />
    </QueryClientProvider>
  </StrictMode>,
)
