import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { getErrorMessages } from './http'

function showErrors(error: unknown) {
  getErrorMessages(error).forEach((message) => toast.error(message))
}

export const queryClient = new QueryClient({
  queryCache: new QueryCache({ onError: showErrors }),
  mutationCache: new MutationCache({ onError: showErrors }),
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
  },
})
