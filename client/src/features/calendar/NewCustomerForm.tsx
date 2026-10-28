import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import type { z } from 'zod'
import { createCustomer, getCustomers } from '@/features/customers/customersApi'
import { customerSchema } from '@/features/customers/schemas'
import type { Customer } from '@/features/customers/types'
import { customersQueryKey } from '@/features/customers/useCustomers'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { maskPhone, withPhoneMask } from '@/features/customers/phone'

// hızlı kayıtta ad, soyad ve telefon yeterli; e-posta ve not müşteri sayfasından eklenir
const quickCustomerSchema = customerSchema.pick({
  firstName: true,
  lastName: true,
  phoneNumber: true,
})

type QuickCustomerValues = z.infer<typeof quickCustomerSchema>

interface NewCustomerFormProps {
  search: string
  onSaved: (customer: Customer) => void
  onCancel: () => void
}

// aramaya yazılan metin forma taşınır: rakamsa telefon, değilse ad soyad
// aramaya yazılan metin forma taşınır: rakamsa telefon, değilse ad soyad
function toDefaultValues(search: string): QuickCustomerValues {
  if (/^[\d\s()+-]+$/.test(search)) {
    return { firstName: '', lastName: '', phoneNumber: maskPhone(search) }
  }
  const [firstName = '', ...rest] = search.split(/\s+/)
  return { firstName, lastName: rest.join(' '), phoneNumber: '' }
}

export function NewCustomerForm({ search, onSaved, onCancel }: NewCustomerFormProps) {
  const queryClient = useQueryClient()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<QuickCustomerValues>({
    resolver: zodResolver(quickCustomerSchema),
    defaultValues: toDefaultValues(search),
  })

  const saveMutation = useMutation({
    mutationFn: async (values: QuickCustomerValues) => {
      await createCustomer({ ...values, email: '', note: '' })
      // kayıt Id döndürmüyor; aynı ad ve telefonla tek kayıt olabildiği için telefonla bulunur
      const matches = await getCustomers(values.phoneNumber)
      const customer = matches.find(
        (c) => c.firstName === values.firstName && c.lastName === values.lastName,
      )
      if (!customer) throw new Error('Kaydedilen müşteri bulunamadı')
      return customer
    },
    onSuccess: (customer) => {
      void queryClient.invalidateQueries({ queryKey: customersQueryKey })
      onSaved(customer)
    },
    meta: { inlineError: true },
  })

  const save = handleSubmit((values) => saveMutation.mutate(values))

  return (
    // randevu formunun içinde olduğu için ayrı <form> değil; Enter bu alanları kaydeder
    <div
      className="flex flex-col gap-3 rounded-xl border border-line bg-sand-soft/40 p-3.5"
      onKeyDown={(event) => {
        if (event.key === 'Enter') {
          event.preventDefault()
          void save()
        }
      }}
    >
      <div className="grid gap-3 sm:grid-cols-2">
        <FormField label="Ad" error={errors.firstName?.message} {...register('firstName')} />
        <FormField label="Soyad" error={errors.lastName?.message} {...register('lastName')} />
      </div>
      <FormField
        label="Telefon"
        prefix="+90"
        type="tel"
        inputMode="tel"
        autoComplete="off"
        placeholder="532 123 45 67"
        error={errors.phoneNumber?.message}
        {...withPhoneMask(register('phoneNumber'))}
      />

      <FormError error={saveMutation.error} />

      <div className="flex justify-end gap-2">
        <button
          type="button"
          onClick={onCancel}
          className="h-9 cursor-pointer rounded-full border border-line bg-surface px-4 text-[13px] font-semibold hover:bg-sand-soft"
        >
          Vazgeç
        </button>
        <button
          type="button"
          onClick={() => void save()}
          disabled={saveMutation.isPending}
          className="h-9 cursor-pointer rounded-full bg-ink px-4 text-[13px] font-semibold text-white disabled:cursor-default disabled:opacity-60"
        >
          {saveMutation.isPending ? 'Kaydediliyor…' : 'Müşteriyi kaydet'}
        </button>
      </div>
    </div>
  )
}
