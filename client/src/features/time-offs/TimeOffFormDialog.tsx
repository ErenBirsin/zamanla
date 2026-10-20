import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useId } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { toast } from 'sonner'
import { todayKey } from '@/features/calendar/time'
import { appointmentsQueryKey } from '@/features/calendar/useDayAppointments'
import { useStaff } from '@/features/staff/useStaff'
import { Dialog } from '@/shared/ui/Dialog'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { type TimeOffValues, timeOffSchema, toTimeOffBody } from './schemas'
import { createTimeOff } from './timeOffsApi'
import { timeOffsQueryKey } from './useTimeOffs'

interface TimeOffFormDialogProps {
  open: boolean
  onClose: () => void
}

function emptyValues(): TimeOffValues {
  const today = todayKey()
  return {
    staffId: '',
    allDay: true,
    startDate: today,
    endDate: today,
    startTime: '09:00',
    endTime: '18:00',
    note: '',
  }
}

export function TimeOffFormDialog({ open, onClose }: TimeOffFormDialogProps) {
  const queryClient = useQueryClient()
  const { data: staff = [] } = useStaff()
  const activeStaff = staff.filter((member) => member.isActive)
  const staffSelectId = useId()
  const noteId = useId()

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
  } = useForm<TimeOffValues>({ resolver: zodResolver(timeOffSchema), defaultValues: emptyValues() })

  const allDay = useWatch({ control, name: 'allDay' })
  const startDate = useWatch({ control, name: 'startDate' })

  useEffect(() => {
    if (open) reset(emptyValues())
  }, [open, reset])

  const saveMutation = useMutation({
    mutationFn: (values: TimeOffValues) => createTimeOff(toTimeOffBody(values)),
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: timeOffsQueryKey })
      // boş saatler değişti
      void queryClient.invalidateQueries({ queryKey: appointmentsQueryKey })
      onClose()
    },
    meta: { inlineError: true },
  })

  const { reset: resetSave } = saveMutation
  useEffect(() => {
    if (open) resetSave()
  }, [open, resetSave])

  return (
    <Dialog open={open} title="İzin ekle" onClose={onClose}>
      <form
        noValidate
        onSubmit={handleSubmit((values) => saveMutation.mutate(values))}
        className="flex flex-col gap-4"
      >
        <div className="flex flex-col gap-1.5">
          <label htmlFor={staffSelectId} className="text-[13px] font-semibold">
            Kimin için
          </label>
          <select
            id={staffSelectId}
            className="h-12 w-full cursor-pointer rounded-xl border border-[#dcd0c1] bg-surface px-3 text-[15px] outline-none focus:border-clay focus:ring-3 focus:ring-clay-soft"
            {...register('staffId')}
          >
            <option value="">Bütün salon (kapalı gün)</option>
            {activeStaff.map((member) => (
              <option key={member.id} value={member.id}>
                {member.firstName} {member.lastName}
              </option>
            ))}
          </select>
        </div>

        <label className="flex items-center gap-2.5 text-sm">
          <input type="checkbox" className="size-4.5 accent-clay" {...register('allDay')} />
          Tüm gün
        </label>

        {allDay ? (
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField
              label="İlk gün"
              type="date"
              min={todayKey()}
              error={errors.startDate?.message}
              {...register('startDate')}
            />
            <FormField
              label="Son gün"
              type="date"
              min={startDate || todayKey()}
              error={errors.endDate?.message}
              {...register('endDate')}
            />
          </div>
        ) : (
          <div className="grid gap-4 sm:grid-cols-3">
            <FormField
              label="Gün"
              type="date"
              min={todayKey()}
              error={errors.startDate?.message}
              {...register('startDate')}
            />
            <FormField
              label="Başlangıç"
              type="time"
              step={300}
              error={errors.startTime?.message}
              {...register('startTime')}
            />
            <FormField
              label="Bitiş"
              type="time"
              step={300}
              error={errors.endTime?.message}
              {...register('endTime')}
            />
          </div>
        )}

        <div className="flex flex-col gap-1.5">
          <label htmlFor={noteId} className="text-[13px] font-semibold">
            Açıklama
          </label>
          <textarea
            id={noteId}
            rows={2}
            placeholder="Örn. Cumhuriyet Bayramı, doktor randevusu"
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
