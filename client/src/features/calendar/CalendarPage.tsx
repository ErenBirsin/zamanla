import { useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { useStaff } from '@/features/staff/useStaff'
import { formatPrice } from '@/shared/ui/format'
import { AppointmentDetailPanel } from './AppointmentDetailPanel'
import { AppointmentFormDialog } from './AppointmentFormDialog'
import { DayGrid } from './DayGrid'
import { addDays, formatDayTitle, todayKey } from './time'
import { useDayAppointments } from './useDayAppointments'
import { useDayTimeOffs } from '@/features/time-offs/useTimeOffs'
import { usePermissions } from '@/features/auth/usePermissions'
import { PERMISSIONS } from '@/shared/auth/permissions'

const iconButton =
  'flex size-10 cursor-pointer items-center justify-center rounded-full border border-line bg-surface hover:bg-sand-soft'

function Chevron({ direction }: { direction: 'left' | 'right' }) {
  return (
    <svg
      width="18"
      height="18"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      <path d={direction === 'left' ? 'm15 18-6-6 6-6' : 'm9 18 6-6-6-6'} />
    </svg>
  )
}

export function CalendarPage() {
  // gün URL'de tutulur: sayfa yenilense de geri tuşunda da aynı gün açılır
  const [searchParams, setSearchParams] = useSearchParams()
  const dateKey = searchParams.get('date') ?? todayKey()

  const { data: staff = [], isPending: staffPending } = useStaff()
  const { data: appointments = [], isPending: appointmentsPending } = useDayAppointments(dateKey)
  const { data: timeOffs = [] } = useDayTimeOffs(dateKey)
  const canViewRevenue = usePermissions().can(PERMISSIONS.revenueView)
  const activeStaff = staff.filter((member) => member.isActive)
  const salonClosed = timeOffs.find((timeOff) => timeOff.staffId === null)
  // null: kapalı; boş alana tıklanınca personel ve saat dolu gelir
  const [formStart, setFormStart] = useState<{ staffId?: string; minutes?: number } | null>(null)

  const [selectedId, setSelectedId] = useState<string | null>(null)
  // iptal edilen ya da başka güne taşınan randevu listede yoksa panel kapanır
  const selected = appointments.find((a) => a.id === selectedId) ?? null

  const goTo = (key: string) => {
    setSelectedId(null)
    setSearchParams(key === todayKey() ? {} : { date: key })
  }

  const noShowCount = appointments.filter((a) => a.status === 'Gelmedi').length
  const expectedRevenue = appointments
    .filter((a) => a.status !== 'Gelmedi')
    .reduce((sum, a) => sum + a.total, 0)

  return (
    <>
      <header className="flex flex-wrap items-center gap-x-4 gap-y-3">
        <h1 className="text-3xl font-bold">Takvim</h1>
        <div className="flex items-center gap-2">
          <button
            type="button"
            aria-label="Önceki gün"
            onClick={() => goTo(addDays(dateKey, -1))}
            className={iconButton}
          >
            <Chevron direction="left" />
          </button>
          <button
            type="button"
            onClick={() => goTo(todayKey())}
            className="h-10 cursor-pointer rounded-full border border-line bg-surface px-4 text-sm font-semibold hover:bg-sand-soft"
          >
            Bugün
          </button>
          <button
            type="button"
            aria-label="Sonraki gün"
            onClick={() => goTo(addDays(dateKey, 1))}
            className={iconButton}
          >
            <Chevron direction="right" />
          </button>
        </div>
        <p className="text-[15px] font-semibold first-letter:uppercase">
          {formatDayTitle(dateKey)}
        </p>
        <button
          type="button"
          onClick={() => setFormStart({})}
          className="flex h-11 cursor-pointer items-center gap-2 rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark sm:ml-auto"
        >
          <svg
            width="18"
            height="18"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2.2"
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden
          >
            <path d="M12 5v14M5 12h14" />
          </svg>
          Yeni randevu
        </button>
      </header>

      <div className="flex flex-wrap gap-2.5 text-[13px] text-ink-soft">
        <span className="rounded-full border border-line bg-surface px-3.5 py-2">
          <strong className="text-ink">{appointments.length}</strong> randevu
        </span>
        {canViewRevenue && (
          <span className="rounded-full border border-line bg-surface px-3.5 py-2">
            <strong className="text-ink">{formatPrice(expectedRevenue)}</strong> beklenen ciro
          </span>
        )}
        {salonClosed && (
          <span className="rounded-full border border-clay/30 bg-clay-soft px-3.5 py-2 font-semibold text-clay-dark">
            Salon kapalı{salonClosed.note && ` · ${salonClosed.note}`}
          </span>
        )}
        {noShowCount > 0 && (
          <span className="flex items-center gap-2 rounded-full border border-line bg-surface px-3.5 py-2">
            <span className="size-2 rounded-full bg-clay" />
            <strong className="text-ink">{noShowCount}</strong> gelmedi
          </span>
        )}
      </div>

      {staffPending || appointmentsPending ? (
        <p className="text-sm text-muted">Takvim yükleniyor…</p>
      ) : activeStaff.length === 0 ? (
        <div className="flex max-w-xl flex-col items-start gap-2 rounded-card border border-dashed border-line p-6">
          <h2 className="text-lg font-bold">Takvimde gösterilecek personel yok</h2>
          <p className="text-sm text-muted">
            Randevular personel sütunlarında görünür.{' '}
            <Link to="/app/staff" className="font-semibold text-clay-dark underline">
              Personel
            </Link>{' '}
            sayfasından çalışanlarını ekle.
          </p>
        </div>
      ) : (
        <div className="flex flex-col gap-5 lg:flex-row lg:items-start">
          <div className="min-w-0 flex-1">
            <DayGrid
              dateKey={dateKey}
              staff={activeStaff}
              appointments={appointments}
              timeOffs={timeOffs}
              // geçmiş günlere randevu eklenmez
              onEmptyClick={
                dateKey < todayKey()
                  ? undefined
                  : (staffId, minutes) => setFormStart({ staffId, minutes })
              }
              selectedId={selectedId}
              onSelect={setSelectedId}
            />
          </div>
          {selected && (
            <AppointmentDetailPanel
              // başka karta geçince iptal formu gibi iç durumlar sıfırlansın
              key={selected.id}
              appointment={selected}
              dateKey={dateKey}
              onClose={() => setSelectedId(null)}
              // başka güne taşındıysa o güne geçilir, panel açık kalır
              onRescheduled={(date) => {
                if (date === dateKey) return
                goTo(date)
                setSelectedId(selected.id)
              }}
            />
          )}
        </div>
      )}

      {/* dialog her açılışta sıfırdan kurulur, önceki seçimler kalmaz */}
      {formStart && (
        <AppointmentFormDialog
          initialDate={dateKey < todayKey() ? todayKey() : dateKey}
          initialStaffId={formStart.staffId}
          initialMinutes={formStart.minutes}
          onClose={() => setFormStart(null)}
          onCreated={(date) => {
            setFormStart(null)
            goTo(date)
          }}
        />
      )}
    </>
  )
}
