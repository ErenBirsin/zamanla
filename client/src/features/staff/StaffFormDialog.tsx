import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { Link } from 'react-router'
import { toast } from 'sonner'
import { useServices } from '@/features/services/useServices'
import { Dialog } from '@/shared/ui/Dialog'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { DayScheduleRow } from './DayScheduleRow'
import { defaultSchedule, toSchedule, toWorkingHours } from './schedule'
import { type StaffValues, staffSchema } from './schemas'
import { type StaffRequest, createStaff, updateStaff } from './staffApi'
import { staffColorClasses, staffColorLabels, staffColors, weekDays } from './staffOptions'
import type { Staff } from './types'
import { staffQueryKey } from './useStaff'

interface StaffFormDialogProps {
  open: boolean
  staff: Staff | null // null ise yeni personel eklenir
  onClose: () => void
}

// her çağrıda yeni dizi: form değerleri birbirine karışmasın
function emptyValues(): StaffValues {
  return {
    firstName: '',
    lastName: '',
    color: 'peach',
    salonServiceIds: [],
    schedule: defaultSchedule(),
    isActive: true,
  }
}

function toFormValues(staff: Staff): StaffValues {
  return {
    firstName: staff.firstName,
    lastName: staff.lastName,
    color: staff.color,
    salonServiceIds: staff.salonServiceIds,
    schedule: toSchedule(staff.workingHours),
    isActive: staff.isActive,
  }
}

export function StaffFormDialog({ open, staff, onClose }: StaffFormDialogProps) {
  const queryClient = useQueryClient()
  const { data: services = [] } = useServices()

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<StaffValues>({ resolver: zodResolver(staffSchema), defaultValues: emptyValues() })

  useEffect(() => {
    if (open) reset(staff ? toFormValues(staff) : emptyValues())
  }, [open, staff, reset])

  const saveMutation = useMutation({
    mutationFn: (values: StaffValues) => {
      const request: StaffRequest = {
        firstName: values.firstName,
        lastName: values.lastName,
        color: values.color,
        workingHours: toWorkingHours(values.schedule),
        salonServiceIds: values.salonServiceIds,
      }
      return staff ? updateStaff(staff.id, request, values.isActive) : createStaff(request)
    },
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: staffQueryKey })
      onClose()
    },
    meta: { inlineError: true },
  })

  const { reset: resetSave } = saveMutation
  useEffect(() => {
    if (open) resetSave()
  }, [open, resetSave])

  return (
    <Dialog open={open} title={staff ? 'Personeli düzenle' : 'Yeni personel'} onClose={onClose}>
      <form
        noValidate
        onSubmit={handleSubmit((values) => saveMutation.mutate(values))}
        className="flex flex-col gap-5"
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField label="Ad" error={errors.firstName?.message} {...register('firstName')} />
          <FormField label="Soyad" error={errors.lastName?.message} {...register('lastName')} />
        </div>

        <fieldset className="flex flex-col gap-2">
          <legend className="mb-2 text-[13px] font-semibold">Takvim rengi</legend>
          <div className="flex gap-3">
            {staffColors.map((color) => (
              <label key={color} title={staffColorLabels[color]} className="cursor-pointer">
                <input type="radio" value={color} className="peer sr-only" {...register('color')} />
                <span
                  className={`block size-9 rounded-full border border-line ${staffColorClasses[color]} peer-checked:ring-2 peer-checked:ring-clay peer-checked:ring-offset-2 peer-focus-visible:outline-2 peer-focus-visible:outline-offset-4 peer-focus-visible:outline-clay`}
                />
                <span className="sr-only">{staffColorLabels[color]}</span>
              </label>
            ))}
          </div>
        </fieldset>

        <fieldset className="flex flex-col gap-2">
          <legend className="mb-2 text-[13px] font-semibold">Verdiği hizmetler</legend>
          {services.length === 0 ? (
            <p className="text-sm text-muted">
              Henüz hizmet yok.{' '}
              <Link to="/app/services" className="font-semibold text-clay-dark underline">
                Hizmetler
              </Link>{' '}
              sayfasından ekleyebilirsin.
            </p>
          ) : (
            <Controller
              control={control}
              name="salonServiceIds"
              render={({ field }) => (
                <div className="grid gap-2 sm:grid-cols-2">
                  {services.map((service) => (
                    <label
                      key={service.id}
                      className="flex cursor-pointer items-center gap-2.5 rounded-xl border border-line px-3 py-2.5 text-sm has-checked:border-clay has-checked:bg-clay-soft/40"
                    >
                      <input
                        type="checkbox"
                        className="size-4.5 accent-clay"
                        checked={field.value.includes(service.id)}
                        onChange={(event) =>
                          field.onChange(
                            event.target.checked
                              ? [...field.value, service.id]
                              : field.value.filter((id) => id !== service.id),
                          )
                        }
                      />
                      <span className="min-w-0 flex-1 truncate">{service.name}</span>
                      {!service.isActive && <span className="text-xs text-muted">Pasif</span>}
                    </label>
                  ))}
                </div>
              )}
            />
          )}
        </fieldset>

        <fieldset className="flex min-w-0 flex-col">
          <legend className="mb-2 text-[13px] font-semibold">Haftalık çalışma saatleri</legend>
          <div className="rounded-xl border border-line px-3">
            {weekDays.map((weekDay, index) => (
              <DayScheduleRow
                key={weekDay.day}
                index={index}
                label={weekDay.label}
                control={control}
                register={register}
                errors={errors}
              />
            ))}
          </div>
        </fieldset>

        {staff && (
          <label className="flex items-center gap-2.5 text-sm">
            <input type="checkbox" className="size-4.5 accent-clay" {...register('isActive')} />
            Aktif (randevu sayfasında görünür)
          </label>
        )}

        <FormError error={saveMutation.error} />
        <div className="mt-1 flex justify-end gap-3">
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
