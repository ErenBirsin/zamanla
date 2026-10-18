import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import { toast } from 'sonner'
import { addDays, toDateKey, todayKey } from '@/features/calendar/time'
import type { AvailabilitySlot } from '@/features/calendar/types'
import { rescheduleManagedAppointment } from './bookingApi'
import { DayStrip } from './DayStrip'
import { formatDayLong, formatSlot } from './format'
import { worksOn } from './hours'
import { SlotList } from './SlotList'
import { StaffPicker } from './StaffPicker'
import type { PublicAppointment, PublicSalon } from './types'
import { publicQueryKey } from './usePublicSalon'
import { useRescheduleAvailability } from './useRescheduleAvailability'

interface RescheduleSectionProps {
  slug: string
  token: string
  salon: PublicSalon
  appointment: PublicAppointment
  onClose: () => void
}

export function RescheduleSection({
  slug,
  token,
  salon,
  appointment,
  onClose,
}: RescheduleSectionProps) {
  const queryClient = useQueryClient()
  const ref = useRef<HTMLElement>(null)

  // telefonda bölüm sayfanın altında açılıyor; görünür alana kaydır
  useEffect(() => {
    ref.current?.scrollIntoView({ block: 'start', behavior: 'smooth' })
  }, [])

  const serviceIds = appointment.services.map((service) => service.id)
  // randevudaki hizmetlerin hepsini veren personel
  const eligibleStaff = salon.staff.filter((member) =>
    serviceIds.every((id) => member.salonServiceIds.includes(id)),
  )
  const [staffId, setStaffId] = useState(
    eligibleStaff.some((member) => member.id === appointment.staffId) ? appointment.staffId : '',
  )
  // yetişemeyen müşteri çoğunlukla aynı gün başka saate bakar
  const [date, setDate] = useState(toDateKey(appointment.startAt))
  const [slot, setSlot] = useState<AvailabilitySlot | null>(null)

  const selectedStaff = eligibleStaff.find((member) => member.id === staffId)
  const candidates = selectedStaff ? [selectedStaff] : eligibleStaff
  const today = todayKey()
  const days = Array.from({ length: salon.bookingDaysAhead + 1 }, (_, index) =>
    addDays(today, index),
  )
  const isOpen = (day: string) => candidates.some((member) => worksOn(member, day))
  const selectedDate = isOpen(date) ? date : (days.find(isOpen) ?? today)

  const availability = useRescheduleAvailability(slug, token, selectedDate, selectedStaff?.id ?? '')
  // mevcut saat listede boş görünür ama oraya taşımak bir şey değiştirmez
  const slots = (availability.data ?? []).filter(
    (item) =>
      !(
        new Date(item.startAt).getTime() === new Date(appointment.startAt).getTime() &&
        item.staffId === appointment.staffId
      ),
  )

  const rescheduleMutation = useMutation({
    mutationFn: (target: AvailabilitySlot) =>
      rescheduleManagedAppointment(slug, token, target.staffId, target.startAt),
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: publicQueryKey(slug) })
      onClose()
    },
    // saat bu arada dolduysa liste yenilenir
    onError: () => {
      setSlot(null)
      void queryClient.invalidateQueries({ queryKey: publicQueryKey(slug) })
    },
  })

  return (
    <section
      ref={ref}
      className="flex flex-col gap-5 rounded-[20px] border border-line bg-surface p-4.5"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 className="text-xl font-bold">Saati değiştir</h2>
          <span className="text-[13px] text-muted">
            Şu anki saat: {formatSlot(appointment.startAt)}
          </span>
        </div>
        <button
          type="button"
          onClick={onClose}
          className="h-9 cursor-pointer rounded-full border border-line px-3.5 text-[13px] font-semibold hover:bg-sand-soft"
        >
          Vazgeç
        </button>
      </div>

      <StaffPicker
        staff={eligibleStaff}
        value={selectedStaff?.id ?? ''}
        onChange={(id) => {
          setStaffId(id)
          setSlot(null)
        }}
      />
      <DayStrip
        days={days}
        value={selectedDate}
        isOpen={isOpen}
        onChange={(day) => {
          setDate(day)
          setSlot(null)
        }}
      />

      {availability.isPending ? (
        <p className="text-sm text-muted">Boş saatler yükleniyor…</p>
      ) : slots.length === 0 ? (
        <p className="text-sm text-muted">
          {formatDayLong(selectedDate)} için başka boş saat yok. Başka bir gün seçebilirsin.
        </p>
      ) : (
        <SlotList slots={slots} value={slot?.startAt ?? null} onChange={setSlot} />
      )}

      <div className="flex flex-col gap-2">
        {slot && (
          <p className="text-center text-[13px] text-ink-soft">
            Yeni saat: <strong>{formatSlot(slot.startAt)}</strong> · {slot.staffFullName}
          </p>
        )}
        <button
          type="button"
          disabled={!slot || rescheduleMutation.isPending}
          onClick={() => slot && rescheduleMutation.mutate(slot)}
          className="h-12 cursor-pointer rounded-full bg-clay text-[15px] font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-50"
        >
          {rescheduleMutation.isPending ? 'Kaydediliyor…' : 'Saati değiştir'}
        </button>
      </div>
    </section>
  )
}
