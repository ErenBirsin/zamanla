import { useMutation, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { type FormEvent, useId, useState } from 'react'
import { toast } from 'sonner'
import { usePermissions } from '@/features/auth/usePermissions'
import { useStaff } from '@/features/staff/useStaff'
import { PERMISSIONS } from '@/shared/auth/permissions'
import { Dialog } from '@/shared/ui/Dialog'
import { FormError } from '@/shared/ui/FormError'
import { rescheduleAppointment } from './appointmentsApi'
import { formatClock, formatDayTitle, toDateKey, todayKey } from './time'
import type { Appointment, AvailabilitySlot } from './types'
import { useAvailability } from './useAvailability'
import { appointmentsQueryKey } from './useDayAppointments'

interface RescheduleDialogProps {
  appointment: Appointment
  onClose: () => void
  onRescheduled: (date: string) => void
}

const sectionTitle = 'text-[13px] font-semibold'

const sameTime = (a: string, b: string) => new Date(a).getTime() === new Date(b).getTime()

export function RescheduleDialog({ appointment, onClose, onRescheduled }: RescheduleDialogProps) {
  const queryClient = useQueryClient()
  const staffSelectId = useId()
  const dateId = useId()

  const salonServiceIds = appointment.salonServices.map((service) => service.salonServiceId)
  const { data: staff = [] } = useStaff()
  // yetkisi olmayan personel randevuyu başka personele taşıyamaz
  const manageAll = usePermissions().can(PERMISSIONS.appointmentsManageAll)
  // süre ve hizmetler değişmez; yalnızca hepsini veren aktif personele taşınabilir
  const eligibleStaff = staff.filter(
    (member) =>
      member.isActive &&
      (manageAll || member.id === appointment.staffId) &&
      salonServiceIds.every((id) => member.salonServiceIds.includes(id)),
  )

  const [pickedStaffId, setStaffId] = useState(appointment.staffId)
  // randevunun personeli artık bu hizmetleri vermiyorsa "fark etmez" ile başlanır;
  // yetkisi olmayan personelde kendi kaydı kalır, backend uygun saat olmadığını söyler
  const staffId =
    !manageAll || staff.length === 0 || eligibleStaff.some((member) => member.id === pickedStaffId)
      ? pickedStaffId
      : ''
  const [date, setDate] = useState(() => {
    const key = toDateKey(appointment.startAt)
    return key < todayKey() ? todayKey() : key
  })
  const [slot, setSlot] = useState<AvailabilitySlot | null>(null)

  const availability = useAvailability(date, salonServiceIds, staffId, appointment.id)
  const slots = availability.data ?? []

  const isCurrent = (item: AvailabilitySlot) =>
    item.staffId === appointment.staffId && sameTime(item.startAt, appointment.startAt)

  const saveMutation = useMutation({
    mutationFn: (selected: AvailabilitySlot) =>
      rescheduleAppointment(appointment.id, selected.staffId, selected.startAt),
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: appointmentsQueryKey })
      onRescheduled(date)
    },
    onError: (error) => {
      // saat bu arada dolduysa liste yenilensin
      if (isAxiosError(error) && error.response?.status === 409) {
        setSlot(null)
        void queryClient.invalidateQueries({ queryKey: appointmentsQueryKey })
      }
    },
    meta: { inlineError: true },
  })

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    if (slot) saveMutation.mutate(slot)
  }

  const duration = appointment.salonServices.reduce((sum, service) => sum + service.duration, 0)

  return (
    <Dialog open title="Yeniden planla" onClose={onClose}>
      <form noValidate onSubmit={handleSubmit} className="flex flex-col gap-5">
        <div className="flex flex-col gap-0.5 rounded-xl bg-sand p-3 text-sm">
          <span className="font-semibold">
            {appointment.customerFullName} · {duration} dk
          </span>
          <span className="text-muted first-letter:uppercase">
            Mevcut: {formatDayTitle(toDateKey(appointment.startAt))},{' '}
            {formatClock(appointment.startAt)} · {appointment.staffFullName}
          </span>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="flex flex-col gap-1.5">
            <label htmlFor={staffSelectId} className={sectionTitle}>
              Personel
            </label>
            <select
              id={staffSelectId}
              value={staffId}
              onChange={(event) => {
                setStaffId(event.target.value)
                setSlot(null)
              }}
              className="h-12 w-full cursor-pointer rounded-xl border border-[#dcd0c1] bg-surface px-3 text-[15px] outline-none focus:border-clay focus:ring-3 focus:ring-clay-soft"
            >
              {manageAll && <option value="">Fark etmez</option>}
              {eligibleStaff.map((member) => (
                <option key={member.id} value={member.id}>
                  {member.firstName} {member.lastName}
                </option>
              ))}
            </select>
          </div>
          <div className="flex flex-col gap-1.5">
            <label htmlFor={dateId} className={sectionTitle}>
              Gün
            </label>
            <input
              id={dateId}
              type="date"
              value={date}
              min={todayKey()}
              onChange={(event) => {
                if (!event.target.value) return
                setDate(event.target.value)
                setSlot(null)
              }}
              className="h-12 w-full rounded-xl border border-[#dcd0c1] bg-surface px-3 text-[15px] outline-none focus:border-clay focus:ring-3 focus:ring-clay-soft"
            />
          </div>
        </div>

        <section className="flex flex-col gap-2">
          <h3 className={sectionTitle}>Yeni saat</h3>
          {availability.isPending ? (
            <p className="text-sm text-muted">Boş saatler yükleniyor…</p>
          ) : availability.isError ? (
            <FormError error={availability.error} />
          ) : slots.length === 0 ? (
            <p className="text-sm text-muted">
              Bu gün için boş saat yok. Başka bir gün ya da personel seç.
            </p>
          ) : (
            <div className="grid max-h-44 grid-cols-[repeat(auto-fill,minmax(4.25rem,1fr))] gap-2 overflow-y-auto pr-1">
              {slots.map((item) => {
                const current = isCurrent(item)
                const selected = slot?.startAt === item.startAt
                return (
                  <button
                    key={item.startAt}
                    type="button"
                    aria-pressed={selected}
                    disabled={current}
                    title={current ? 'Randevunun şu anki saati' : undefined}
                    onClick={() => setSlot(item)}
                    className={`h-10 cursor-pointer rounded-full border text-sm font-semibold disabled:cursor-default disabled:border-dashed disabled:text-muted ${selected ? 'border-clay bg-clay text-white' : 'border-line bg-surface hover:bg-sand-soft'}`}
                  >
                    {formatClock(item.startAt)}
                  </button>
                )
              })}
            </div>
          )}
          {slot && (
            <p className="text-[13px] text-muted">
              {formatClock(slot.startAt)} · {slot.staffFullName}
            </p>
          )}
        </section>

        <FormError error={saveMutation.error} />

        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            className="h-11 cursor-pointer rounded-full border border-line px-5 text-sm font-semibold hover:bg-sand-soft"
          >
            Vazgeç
          </button>
          <button
            type="submit"
            disabled={!slot || saveMutation.isPending}
            className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
          >
            {saveMutation.isPending ? 'Kaydediliyor…' : 'Taşı'}
          </button>
        </div>
      </form>
    </Dialog>
  )
}
