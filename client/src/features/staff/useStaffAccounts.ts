import { useQuery } from '@tanstack/react-query'
import { usePermissions } from '@/features/auth/usePermissions'
import { getPermissionCatalog, getStaffAccounts } from './staffAccountsApi'
import { staffQueryKey } from './useStaff'

// personel önekinin altında; personel silinince ya da pasif olunca hesap listesi de yenilenir
export const staffAccountsQueryKey = [...staffQueryKey, 'accounts']

// uç sadece sahibe açık; personelde hiç istek atılmaz
export function useStaffAccounts() {
  const { isOwner } = usePermissions()
  return useQuery({
    queryKey: staffAccountsQueryKey,
    queryFn: getStaffAccounts,
    enabled: isOwner,
  })
}

// katalog kodda sabit, oturum boyunca değişmez
export function usePermissionCatalog() {
  return useQuery({
    queryKey: ['permissions'],
    queryFn: getPermissionCatalog,
    staleTime: Infinity,
  })
}
