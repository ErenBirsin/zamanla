import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getCustomers } from './customersApi'

// geçersiz kılarken bütün aramaları kapsayan ortak önek
export const customersQueryKey = ['customers']

export function useCustomers(search: string, enabled = true) {
  return useQuery({
    queryKey: [...customersQueryKey, search],
    queryFn: () => getCustomers(search),
    enabled,
    // yeni arama gelene kadar önceki sonuçlar ekranda kalır, tablo boşalıp dolmaz
    placeholderData: keepPreviousData,
  })
}
