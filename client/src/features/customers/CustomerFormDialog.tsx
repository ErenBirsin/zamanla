import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useId } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { Dialog } from '@/shared/ui/Dialog'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { createCustomer, updateCustomer } from './customersApi'
import { formatPhone } from './phone'
import { type CustomerValues, customerSchema } from './schemas'
import type { Customer } from './types'
import { customersQueryKey } from './useCustomers'

interface CustomerFormDialogProps {
  open: boolean
  customer: Customer | null
  onClose: () => void
}

const emptyValues: CustomerValues = {
  firstName: '',
  lastName: '',
  phoneNumber: '',
  email: '',
  note: '',
}

function toFormValues(customer: Customer): CustomerValues {
  return {
    firstName: customer.firstName,
    lastName: customer.lastName,
    phoneNumber: formatPhone(customer.phoneNumber),
    email: customer.email ?? '',
    note: customer.note ?? '',
  }
}

export function CustomerFormDialog({ open, customer, onClose }: CustomerFormDialogProps) {
  const queryClient = useQueryClient()
  const noteId = useId()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CustomerValues>({ resolver: zodResolver(customerSchema), defaultValues: emptyValues })

  useEffect(() => {
    if (open) reset(customer ? toFormValues(customer) : emptyValues)
  }, [open, customer, reset])

  const saveMutation = useMutation({
    mutationFn: (values: CustomerValues) =>
      customer ? updateCustomer(customer.id, values) : createCustomer(values),
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: customersQueryKey })
      onClose()
    },
    meta: { inlineError: true },
  })

  const { reset: resetSave } = saveMutation
  useEffect(() => {
    if (open) resetSave()
  }, [open, resetSave])

  return (
    <Dialog open={open} title={customer ? 'Müşteriyi düzenle' : 'Yeni müşteri'} onClose={onClose}>
      <form
        noValidate
        onSubmit={handleSubmit((values) => saveMutation.mutate(values))}
        className="flex flex-col gap-4"
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField label="Ad" error={errors.firstName?.message} {...register('firstName')} />
          <FormField label="Soyad" error={errors.lastName?.message} {...register('lastName')} />
        </div>
        <FormField
          label="Telefon"
          type="tel"
          inputMode="tel"
          autoComplete="off"
          placeholder="0532 123 45 67"
          error={errors.phoneNumber?.message}
          {...register('phoneNumber')}
        />
        <FormField
          label="E-posta"
          type="email"
          autoComplete="off"
          hint="İsteğe bağlı; hatırlatma göndermek için kullanılır."
          error={errors.email?.message}
          {...register('email')}
        />

        <div className="flex flex-col gap-1.5">
          <label htmlFor={noteId} className="text-[13px] font-semibold">
            Not
          </label>
          <textarea
            id={noteId}
            rows={3}
            placeholder="Örn. boya 6/1, amonyak alerjisi var"
            aria-invalid={errors.note ? true : undefined}
            className="w-full resize-y rounded-xl border border-[#dcd0c1] bg-surface px-3.5 py-3 text-[15px] outline-none placeholder:text-muted/60 focus:border-clay focus:ring-3 focus:ring-clay-soft aria-invalid:border-clay"
            {...register('note')}
          />
          {errors.note && <p className="text-[13px] text-clay-dark">{errors.note.message}</p>}
        </div>

        <FormError error={saveMutation.error} />
        <div className="mt-2 flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            className="h-11 cursor-pointer rounded-full border border-line px-5 text-sm font-semibold hover:bg-sand-soft"
          >
            Vazgeç
          </button>
          <button
            type="submit"
            disabled={saveMutation.isPending}
            className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
          >
            {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
          </button>
        </div>
      </form>
    </Dialog>
  )
}
