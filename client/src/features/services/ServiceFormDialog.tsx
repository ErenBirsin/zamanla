import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { Dialog } from '@/shared/ui/Dialog'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { type ServiceValues, serviceSchema } from './schemas'
import { createService, updateService } from './servicesApi'
import type { SalonService } from './types'
import { servicesQueryKey } from './useServices'

interface ServiceFormDialogProps {
  open: boolean
  // null ise yeni hizmet eklenir
  service: SalonService | null
  categories: string[]
  onClose: () => void
}

const emptyValues: ServiceValues = {
  name: '',
  category: '',
  durationMinutes: 30,
  bufferMinutes: 0,
  price: 0,
  isActive: true,
}

export function ServiceFormDialog({ open, service, categories, onClose }: ServiceFormDialogProps) {
  const queryClient = useQueryClient()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ServiceValues>({ resolver: zodResolver(serviceSchema), defaultValues: emptyValues })

  // form her açılışta seçilen hizmetin değerleriyle (ya da boş) doldurulur
  useEffect(() => {
    if (open) reset(service ?? emptyValues)
  }, [open, service, reset])

  const saveMutation = useMutation({
    mutationFn: (values: ServiceValues) =>
      service ? updateService(service.id, values) : createService(values),
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: servicesQueryKey })
      onClose()
    },
    meta: { inlineError: true },
  })

  const { reset: resetSave } = saveMutation
  useEffect(() => {
    if (open) resetSave()
  }, [open, resetSave])

  return (
    <Dialog open={open} title={service ? 'Hizmeti düzenle' : 'Yeni hizmet'} onClose={onClose}>
      <form
        noValidate
        onSubmit={handleSubmit((values) => saveMutation.mutate(values))}
        className="flex flex-col gap-4"
      >
        <FormField
          label="Hizmet adı"
          placeholder="Saç kesimi"
          error={errors.name?.message}
          {...register('name')}
        />
        <FormField
          label="Kategori"
          placeholder="Saç"
          list="service-categories"
          error={errors.category?.message}
          {...register('category')}
        />
        <datalist id="service-categories">
          {categories.map((category) => (
            <option key={category} value={category} />
          ))}
        </datalist>

        <div className="grid gap-4 sm:grid-cols-3">
          <FormField
            label="Süre (dk)"
            type="number"
            inputMode="numeric"
            min={5}
            step={5}
            error={errors.durationMinutes?.message}
            {...register('durationMinutes', { valueAsNumber: true })}
          />
          <FormField
            label="Hazırlık (dk)"
            type="number"
            inputMode="numeric"
            min={0}
            step={5}
            error={errors.bufferMinutes?.message}
            {...register('bufferMinutes', { valueAsNumber: true })}
          />
          <FormField
            label="Fiyat (₺)"
            type="number"
            inputMode="decimal"
            min={0}
            step={0.01}
            error={errors.price?.message}
            {...register('price', { valueAsNumber: true })}
          />
        </div>

        {service && (
          <label className="flex items-center gap-2.5 text-sm">
            <input type="checkbox" className="size-4.5 accent-clay" {...register('isActive')} />
            Aktif (randevu sayfasında görünür)
          </label>
        )}

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
