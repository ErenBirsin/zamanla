import { addDays, todayKey } from '@/features/calendar/time'
import type { AvailabilitySlot } from '@/features/calendar/types'
import { DayStrip } from './DayStrip'
import { formatDayLong } from './format'
import { fullName, worksOn } from './hours'
import { SlotList } from './SlotList'
import { StaffPicker } from './StaffPicker'
import { StepHeading } from './StepHeading'
import type { PublicSalon } from './types'
import { usePublicAvailability } from './usePublicAvailability'

interface TimeStepProps {
  slug: string
  salon: PublicSalon
  salonServiceIds: string[]
  staffId: string
  onStaffChange: (staffId: string) => void
  date: string | null
  onDateChange: (date: string) => void
  slot: AvailabilitySlot | null
  onSlotChange: (slot: AvailabilitySlot) => void
}

export function TimeStep({
  slug,
  salon,
  salonServiceIds,
  staffId,
  onStaffChange,
  date,
  onDateChange,
  slot,
  onSlotChange,
}: TimeStepProps) {
  // seçilen hizmetlerin hepsini veren personel
  const eligibleStaff = salon.staff.filter((member) =>
    salonServiceIds.every((id) => member.salonServiceIds.includes(id)),
  )
  // hizmetler değişip personel artık uygun değilse "fark etmez" sayılır
  const selectedStaff = eligibleStaff.find((member) => member.id === staffId)
  const candidates = selectedStaff ? [selectedStaff] : eligibleStaff

  const today = todayKey()
  const days = Array.from({ length: salon.bookingDaysAhead + 1 }, (_, index) =>
    addDays(today, index),
  )
  const isOpen = (day: string) => candidates.some((member) => worksOn(member, day))
  // seçilen gün yeni personelin kapalı günüyse ilk açık güne geçilir
  const selectedDate = date && isOpen(date) ? date : (days.find(isOpen) ?? today)

  const availability = usePublicAvailability(
    slug,
    selectedDate,
    salonServiceIds,
    selectedStaff?.id ?? '',
  )
  const slots = availability.data ?? []

  return (
    <section className="flex flex-col gap-5">
      <StepHeading step={2} title="Kiminle ve ne zaman?" />

      {eligibleStaff.length === 0 ? (
        <p className="rounded-2xl border border-line bg-surface p-4 text-sm text-muted">
          Seçtiğin hizmetlerin hepsini yapan bir personel yok. Geri dönüp hizmetleri azaltabilirsin.
        </p>
      ) : (
        <>
          <StaffPicker
            staff={eligibleStaff}
            value={selectedStaff?.id ?? ''}
            onChange={onStaffChange}
          />
          <DayStrip days={days} value={selectedDate} isOpen={isOpen} onChange={onDateChange} />

          <div className="flex flex-col gap-4">
            {availability.isPending ? (
              <p className="text-sm text-muted">Boş saatler yükleniyor…</p>
            ) : slots.length === 0 ? (
              <p className="text-sm text-muted">
                {formatDayLong(selectedDate)} için boş saat kalmadı. Başka bir gün seçebilirsin.
              </p>
            ) : (
              <>
                <p className="text-[13px] text-muted">
                  {selectedStaff && `${fullName(selectedStaff)} için `}
                  {formatDayLong(selectedDate)} günü <strong>{slots.length} uygun saat</strong> var.
                </p>
                <SlotList slots={slots} value={slot?.startAt ?? null} onChange={onSlotChange} />
              </>
            )}
          </div>
        </>
      )}
    </section>
  )
}
