import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { toast } from 'sonner'
import { useDebouncedValue } from '@/shared/hooks/useDebouncedValue'
import { CustomerFormDialog } from './CustomerFormDialog'
import { deleteCustomer } from './customersApi'
import { formatPhone } from './phone'
import type { Customer } from './types'
import { customersQueryKey, useCustomers } from './useCustomers'
import { usePermissions } from '@/features/auth/usePermissions'
import { PERMISSIONS } from '@/shared/auth/permissions'
import { confirmAction } from '@/shared/ui/confirm'
import { ListSkeleton } from '@/shared/ui/Skeleton'
import { QueryError } from '@/shared/ui/QueryError'
import { ActionButton } from '@/shared/ui/ActionButton'

export function CustomersPage() {
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search.trim())
  const {
    data: customers = [],
    isPending,
    isFetching,
    isError,
    error,
    refetch,
  } = useCustomers(debouncedSearch)
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<Customer | null>(null)
  // ekleme ve düzenleme herkese açık, randevu alırken gerekiyor
  const canDelete = usePermissions().can(PERMISSIONS.customersDelete)

  const deleteMutation = useMutation({
    mutationFn: deleteCustomer,
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: customersQueryKey })
    },
  })

  const openCreate = () => {
    setEditing(null)
    setFormOpen(true)
  }

  const openEdit = (customer: Customer) => {
    setEditing(customer)
    setFormOpen(true)
  }

  const remove = (customer: Customer) => {
    confirmAction({
      title: 'Müşteri silinsin mi?',
      message: `"${customer.fullName}" kaydının adı, telefonu, e-postası ve notu kalıcı olarak silinir; geçmiş randevular isimsiz olarak kalır.`,
      confirmLabel: 'Sil',
      onConfirm: () => deleteMutation.mutate(customer.id),
    })
  }

  return (
    <>
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div className="flex flex-col gap-1.5">
          <h1 className="text-3xl font-bold">Müşteriler</h1>
          <p className="text-muted">Salonuna gelen müşterilerin iletişim bilgileri ve notları.</p>
        </div>
        <button
          type="button"
          onClick={openCreate}
          className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark"
        >
          Müşteri ekle
        </button>
      </header>

      <div className="flex max-w-md flex-col gap-1.5">
        <label htmlFor="customer-search" className="sr-only">
          Müşteri ara
        </label>
        <input
          id="customer-search"
          type="search"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="İsim ya da telefon ara"
          autoComplete="off"
          className="h-11 w-full rounded-full border border-[#dcd0c1] bg-surface px-4 text-[15px] outline-none placeholder:text-muted/70 focus:border-clay focus:ring-3 focus:ring-clay-soft"
        />
      </div>

      {isPending ? (
        <ListSkeleton />
      ) : isError ? (
        <QueryError error={error} onRetry={() => void refetch()} />
      ) : customers.length === 0 ? (
        debouncedSearch ? (
          <p className="text-sm text-muted">"{debouncedSearch}" ile eşleşen müşteri yok.</p>
        ) : (
          <div className="flex max-w-xl flex-col items-start gap-2 rounded-card border border-dashed border-line p-6">
            <h2 className="text-lg font-bold">Henüz müşteri yok</h2>
            <p className="text-sm text-muted">
              Telefonla randevu alan müşterilerini buraya ekleyebilirsin. Online randevu alanlar
              ileride kendiliğinden eklenecek.
            </p>
          </div>
        )
      ) : (
        <>
          {/* telefonda tablo yerine kartlar */}
          <ul
            className={`flex flex-col gap-3 transition-opacity md:hidden ${isFetching ? 'opacity-60' : ''}`}
          >
            {customers.map((customer) => (
              <li
                key={customer.id}
                className="flex flex-col gap-3 rounded-card border border-line bg-surface p-4"
              >
                <div className="flex min-w-0 flex-col gap-0.5">
                  <span className="font-semibold">{customer.fullName}</span>
                  <a
                    href={`tel:${customer.phoneNumber}`}
                    className="self-start text-sm text-ink-soft tabular-nums hover:text-clay-dark"
                  >
                    {formatPhone(customer.phoneNumber)}
                  </a>
                  {customer.email && (
                    <span className="truncate text-sm text-ink-soft">{customer.email}</span>
                  )}
                  {customer.note && (
                    <span className="line-clamp-2 text-sm text-muted">{customer.note}</span>
                  )}
                </div>
                <div className="flex gap-2">
                  <ActionButton icon="edit" onClick={() => openEdit(customer)}>
                    Düzenle
                  </ActionButton>
                  {canDelete && (
                    <ActionButton icon="delete" onClick={() => remove(customer)}>
                      Sil
                    </ActionButton>
                  )}
                </div>
              </li>
            ))}
          </ul>

          <div
            className={`relative hidden overflow-x-auto rounded-card border border-line bg-surface transition-opacity md:block ${isFetching ? 'opacity-60' : ''}`}
          >
            <table className="w-full min-w-180 text-left text-sm">
              <thead className="border-b border-line text-xs font-semibold tracking-wide text-muted">
                <tr>
                  <th className="px-5 py-3">MÜŞTERİ</th>
                  <th className="px-5 py-3">TELEFON</th>
                  <th className="px-5 py-3">E-POSTA</th>
                  <th className="px-5 py-3">NOT</th>
                  <th className="px-5 py-3">
                    <span className="sr-only">İşlemler</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {customers.map((customer) => (
                  <tr key={customer.id} className="border-b border-line-soft last:border-b-0">
                    <td className="px-5 py-3.5 font-semibold">{customer.fullName}</td>
                    <td className="px-5 py-3.5 whitespace-nowrap text-ink-soft tabular-nums">
                      <a href={`tel:${customer.phoneNumber}`} className="hover:text-clay-dark">
                        {formatPhone(customer.phoneNumber)}
                      </a>
                    </td>
                    <td className="px-5 py-3.5 text-ink-soft">
                      {customer.email ?? <span className="text-muted">—</span>}
                    </td>
                    <td className="max-w-64 px-5 py-3.5 text-ink-soft">
                      {customer.note ? (
                        <span className="line-clamp-2">{customer.note}</span>
                      ) : (
                        <span className="text-muted">—</span>
                      )}
                    </td>
                    <td className="px-5 py-3.5">
                      <div className="flex justify-end gap-2">
                        <ActionButton icon="edit" onClick={() => openEdit(customer)}>
                          Düzenle
                        </ActionButton>
                        {canDelete && (
                          <ActionButton icon="delete" onClick={() => remove(customer)}>
                            Sil
                          </ActionButton>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      <CustomerFormDialog open={formOpen} customer={editing} onClose={() => setFormOpen(false)} />
    </>
  )
}
