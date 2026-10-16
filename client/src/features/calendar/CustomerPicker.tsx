import { useId, useState } from 'react'
import { formatPhone } from '@/features/customers/phone'
import type { Customer } from '@/features/customers/types'
import { useCustomers } from '@/features/customers/useCustomers'
import { useDebouncedValue } from '@/shared/hooks/useDebouncedValue'

interface CustomerPickerProps {
  value: Customer | null
  onChange: (customer: Customer | null) => void
}

const RESULT_LIMIT = 6

export function CustomerPicker({ value, onChange }: CustomerPickerProps) {
  const inputId = useId()
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search.trim())
  // boş aramada bütün müşteri listesi çekilmesin
  const { data: customers = [], isFetching } = useCustomers(debouncedSearch, debouncedSearch !== '')

  if (value) {
    return (
      <div className="flex items-center gap-3 rounded-xl border border-clay bg-clay-soft/40 px-3.5 py-2.5">
        <div className="flex min-w-0 flex-1 flex-col">
          <span className="truncate text-sm font-semibold">{value.fullName}</span>
          <span className="text-xs text-muted">{formatPhone(value.phoneNumber)}</span>
        </div>
        <button
          type="button"
          onClick={() => onChange(null)}
          className="h-9 shrink-0 cursor-pointer rounded-full border border-line bg-surface px-3.5 text-[13px] font-semibold hover:bg-sand-soft"
        >
          Değiştir
        </button>
      </div>
    )
  }

  const results = customers.slice(0, RESULT_LIMIT)

  return (
    <div className="flex flex-col gap-2">
      <label htmlFor={inputId} className="sr-only">
        Müşteri ara
      </label>
      <input
        id={inputId}
        type="search"
        value={search}
        onChange={(event) => setSearch(event.target.value)}
        placeholder="Telefon ya da isim yaz"
        autoComplete="off"
        className="h-12 w-full rounded-xl border border-[#dcd0c1] bg-surface px-3.5 text-[15px] outline-none placeholder:text-muted/60 focus:border-clay focus:ring-3 focus:ring-clay-soft"
      />

      {debouncedSearch && !isFetching && results.length === 0 && (
        <p className="text-[13px] text-muted">"{debouncedSearch}" ile eşleşen müşteri yok.</p>
      )}

      {debouncedSearch && results.length > 0 && (
        <ul className="flex flex-col divide-y divide-line-soft rounded-xl border border-line">
          {results.map((customer) => (
            <li key={customer.id}>
              <button
                type="button"
                onClick={() => onChange(customer)}
                className="flex w-full cursor-pointer items-center justify-between gap-3 px-3.5 py-2.5 text-left text-sm hover:bg-sand-soft"
              >
                <span className="truncate font-semibold">{customer.fullName}</span>
                <span className="shrink-0 text-xs text-muted">
                  {formatPhone(customer.phoneNumber)}
                </span>
              </button>
            </li>
          ))}
        </ul>
      )}
      {debouncedSearch && customers.length > RESULT_LIMIT && (
        <p className="text-[13px] text-muted">
          {customers.length - RESULT_LIMIT} sonuç daha var, aramayı daralt.
        </p>
      )}
    </div>
  )
}
