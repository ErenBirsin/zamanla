import { useMutation, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { type FormEvent, useId, useState } from 'react'
import { toast } from 'sonner'
import type { Customer } from '@/features/customers/types'
import { useServices } from '@/features/services/useServices'
import { useStaff } from '@/features/staff/useStaff'
import { Dialog } from '@/shared/ui/Dialog'
import { FormError } from '@/shared/ui/FormError'
import { formatPrice } from '@/shared/ui/format'
import { createAppointment } from './appointmentsApi'
import { CustomerPicker } from './CustomerPicker'
import { formatClock, minutesOfDay, minutesToLabel, todayKey } from './time'
import type { AvailabilitySlot } from './types'
import { useAvailability } from './useAvailability'
import { appointmentsQueryKey } from './useDayAppointments'
import { usePermissions } from '@/features/auth/usePermissions'
import { PERMISSIONS } from '@/shared/auth/permissions'

interface AppointmentFormDialogProps {
  initialDate: string
  // takvimde boş alana tıklanınca gelir
  initialStaffId?: string
  initialMinutes?: number
  onClose: () => void
  onCreated: (date: string) => void
}

const sectionTitle = 'text-[13px] font-semibold'

export function AppointmentFormDialog({
  initialDate,
  initialStaffId = '',
  initialMinutes,
  onClose,
  onCreated,
}: AppointmentFormDialogProps) {
  const queryClient = useQueryClient()
  const dateId = useId()
  const staffId = useId()
  const noteId = useId()
  const { staffId: ownStaffId, can } = usePermissions()
  // yetkisi olmayan personel sadece kendi adına randevu açar
  const manageAll = can(PERMISSIONS.appointmentsManageAll)

  const [customer, setCustomer] = useState<Customer | null>(null)
  // seçim sırası önemli: hizmetler bu sırayla arka arkaya yapılır
  const [salonServiceIds, setSalonServiceIds] = useState<string[]>([])
  const [selectedStaffId, setSelectedStaffId] = useState(
    manageAll ? initialStaffId : (ownStaffId ?? ''),
  )
  const [date, setDate] = useState(initialDate)
  const [pickedSlot, setSlot] = useState<AvailabilitySlot | null>(null)
  // takvimde tıklanan saat; boş saatler gelince uygunsa kendiliğinden seçilir
  const [preferredMinutes, setPreferredMinutes] = useState(initialMinutes)
  const [note, setNote] = useState('')

  const { data: services = [] } = useServices()
  const { data: staff = [] } = useStaff()
  const activeStaff = staff.filter(
    (member) => member.isActive && (manageAll || member.id === ownStaffId),
  )

  // en az bir aktif personelin verdiği aktif hizmetler
  const bookableServices = services.filter(
    (service) =>
      service.isActive && activeStaff.some((member) => member.salonServiceIds.includes(service.id)),
  )
  const selectedServices = salonServiceIds
    .map((id) => bookableServices.find((service) => service.id === id))
    .filter((service) => service !== undefined)
  const totalDuration = selectedServices.reduce((sum, service) => sum + service.durationMinutes, 0)
  const totalPrice = selectedServices.reduce((sum, service) => sum + service.price, 0)

  // seçilen hizmetlerin hepsini veren personel
  const eligibleStaff = activeStaff.filter((member) =>
    salonServiceIds.every((id) => member.salonServiceIds.includes(id)),
  )

  const availability = useAvailability(date, salonServiceIds, selectedStaffId)
  const slots = availability.data ?? []
  const preferredSlot =
    preferredMinutes === undefined
      ? undefined
      : slots.find((item) => minutesOfDay(item.startAt) === preferredMinutes)
  const slot = pickedSlot ?? preferredSlot ?? null

  const toggleService = (id: string) => {
    const next = salonServiceIds.includes(id)
      ? salonServiceIds.filter((serviceId) => serviceId !== id)
      : [...salonServiceIds, id]
    setSalonServiceIds(next)
    setSlot(null)
    // seçili personel yeni hizmetlerden birini vermiyorsa "fark etmez"e dön
    const member = activeStaff.find((m) => m.id === selectedStaffId)
    if (member && !next.every((serviceId) => member.salonServiceIds.includes(serviceId))) {
      setSelectedStaffId('')
    }
  }

  const saveMutation = useMutation({
    mutationFn: createAppointment,
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: appointmentsQueryKey })
      onCreated(date)
    },
    onError: (error) => {
      // saat bu arada dolduysa takvim ve boş saatler yenilensin, kullanıcı başka saat seçsin
      if (isAxiosError(error) && error.response?.status === 409) {
        setSlot(null)
        setPreferredMinutes(undefined)
        void queryClient.invalidateQueries({ queryKey: appointmentsQueryKey })
      }
    },
    meta: { inlineError: true },
  })

  const canSave = customer !== null && salonServiceIds.length > 0 && slot !== null

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    if (!customer || !slot) return
    saveMutation.mutate({
      customerId: customer.id,
      staffId: slot.staffId,
      salonServiceIds,
      startAt: slot.startAt,
      note: note.trim(),
    })
  }

  return (
    <Dialog open title="Yeni randevu" onClose={onClose}>
      <form noValidate onSubmit={handleSubmit} className="flex flex-col gap-5">
        <section className="flex flex-col gap-2">
          <h3 className={sectionTitle}>Müşteri</h3>
          <CustomerPicker value={customer} onChange={setCustomer} />
        </section>

        <fieldset className="flex min-w-0 flex-col gap-2">
          <legend className={`${sectionTitle} mb-2`}>Hizmetler</legend>
          {bookableServices.length === 0 ? (
            <p className="text-sm text-muted">
              Randevu verilebilecek hizmet yok. Personel sayfasından çalışanlara hizmet ata.
            </p>
          ) : (
            <div className="grid gap-2 sm:grid-cols-2">
              {bookableServices.map((service) => (
                <label
                  key={service.id}
                  className="flex cursor-pointer items-center gap-2.5 rounded-xl border border-line px-3 py-2.5 text-sm has-checked:border-clay has-checked:bg-clay-soft/40"
                >
                  <input
                    type="checkbox"
                    className="size-4.5 accent-clay"
                    checked={salonServiceIds.includes(service.id)}
                    onChange={() => toggleService(service.id)}
                  />
                  <span className="min-w-0 flex-1 truncate">{service.name}</span>
                  <span className="shrink-0 text-xs text-muted">{service.durationMinutes} dk</span>
                </label>
              ))}
            </div>
          )}
          {selectedServices.length > 0 && (
            <p className="text-[13px] text-muted">
              Toplam {totalDuration} dk · {formatPrice(totalPrice)}
            </p>
          )}
        </fieldset>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="flex flex-col gap-1.5">
            <label htmlFor={staffId} className={sectionTitle}>
              Personel
            </label>
            <select
              id={staffId}
              value={selectedStaffId}
              onChange={(event) => {
                setSelectedStaffId(event.target.value)
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
                setPreferredMinutes(undefined)
              }}
              className="h-12 w-full rounded-xl border border-[#dcd0c1] bg-surface px-3 text-[15px] outline-none focus:border-clay focus:ring-3 focus:ring-clay-soft"
            />
          </div>
        </div>

        <section className="flex flex-col gap-2">
          <h3 className={sectionTitle}>Saat</h3>
          {salonServiceIds.length === 0 ? (
            <p className="text-sm text-muted">
              Boş saatleri görmek için hizmet seç.
              {preferredMinutes !== undefined &&
                ` ${minutesToLabel(preferredMinutes)} uygunsa seçili gelir.`}
            </p>
          ) : availability.isPending ? (
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
                const selected = slot?.startAt === item.startAt
                return (
                  <button
                    key={item.startAt}
                    type="button"
                    aria-pressed={selected}
                    onClick={() => setSlot(item)}
                    className={`h-10 cursor-pointer rounded-full border text-sm font-semibold ${selected ? 'border-clay bg-clay text-white' : 'border-line bg-surface hover:bg-sand-soft'}`}
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
          {!slot && preferredMinutes !== undefined && availability.isSuccess && (
            <p className="text-[13px] text-clay-dark">
              {minutesToLabel(preferredMinutes)} bu hizmetler için uygun değil, başka bir saat seç.
            </p>
          )}
        </section>

        <div className="flex flex-col gap-1.5">
          <label htmlFor={noteId} className={sectionTitle}>
            Not
          </label>
          <textarea
            id={noteId}
            rows={2}
            maxLength={500}
            value={note}
            onChange={(event) => setNote(event.target.value)}
            placeholder="İsteğe bağlı"
            className="w-full resize-y rounded-xl border border-[#dcd0c1] bg-surface px-3.5 py-3 text-[15px] outline-none placeholder:text-muted/60 focus:border-clay focus:ring-3 focus:ring-clay-soft"
          />
        </div>

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
            disabled={!canSave || saveMutation.isPending}
            className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
          >
            {saveMutation.isPending ? 'Kaydediliyor…' : 'Randevuyu kaydet'}
          </button>
        </div>
      </form>
    </Dialog>
  )
}
