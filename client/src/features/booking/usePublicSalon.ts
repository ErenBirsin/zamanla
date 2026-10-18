import { useQuery } from '@tanstack/react-query'
import { getPublicSalon } from './bookingApi'

// salonun bütün public sorguları bu anahtarla başlar; kayıttan sonra hepsi birlikte yenilenir
export const publicQueryKey = (slug: string) => ['public', slug]

export function usePublicSalon(slug: string) {
  return useQuery({
    queryKey: publicQueryKey(slug),
    queryFn: () => getPublicSalon(slug),
    // bulunamayan salon için sayfada mesaj gösterilir
    meta: { inlineError: true },
    retry: false,
  })
}
