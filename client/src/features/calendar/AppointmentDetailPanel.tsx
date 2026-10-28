import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useId, useRef, useState } from 'react'
import { toast } from 'sonner'
import { formatPhone } from '@/features/customers/phone'
import { staffColorClasses } from '@/features/staff/staffOptions'
import { formatPrice } from '@/shared/ui/format'
import { cancelAppointment, completeAppointment, markAppointmentNoShow } from './appointmentsApi'
import { RescheduleDialog } from './RescheduleDialog'
import { formatClock, formatDateTime, formatDayTitle, todayKey } from './time'
import type { Appointment } from './types'
import { appointmentsQueryKey } from './useDayAppointments'
import { useNow } from './useNow'
import { usePermissions } from '@/features/auth/usePermissions'

interface AppointmentDetailPanelProps {
  appointment: Appointment
  dateKey: string
  onClose: () => void
  onRescheduled: (date: string) => void
}

// bu sayıdan sonra müşteri için uyarı gösterilir
const NO_SHOW_WARNING = 3

const sectionTitle = 'text-xs font-semibold tracking-wide text-muted uppercase'

const statusStyles: Record<Appointment['status'], string> = {
  Onaylandı: 'bg-sage-soft text-sage',
  Tamamlandı: 'bg-sand text-muted',
  Gelmedi: 'bg-clay-soft text-clay-dark',
  'İptal Edildi': 'bg-sand text-muted',
}

export function AppointmentDetailPanel({
  appointment,
  dateKey,
  onClose,
  onRescheduled,
}: AppointmentDetailPanelProps) {
  const queryClient = useQueryClient()
  const reasonId = useId()
  const now = useNow()
  // gelmedi ve iptal geri alınamaz; önce panelde onay istenir
  const [confirming, setConfirming] = useState<'no-show' | 'cancel' | null>(null)
  const [reason, setReason] = useState('')
  const [rescheduling, setRescheduling] = useState(false)
  const ref = useRef<HTMLElement>(null)

  // mobilde panel takvimin altında kalıyor; seçilince görünür alana kaydır
  useEffect(() => {
    ref.current?.scrollIntoView({ block: 'nearest', behavior: 'smooth' })
  }, [appointment.id])

  const isOpen = appointment.status === 'Onaylandı'
  // başka personelin randevusu yetki yoksa sadece görüntülenir
  const canManage = usePermissions().canManageAppointment(appointment.staffId)
  // tamamlandı ve gelmedi ancak randevu saati gelince işaretlenebilir
  const hasStarted = new Date(appointment.startAt) <= now
  const services = appointment.salonServices.map((service) => service.name).join(' + ')
  const day = dateKey === todayKey() ? 'Bugün' : formatDayTitle(dateKey)
  const histories = [...appointment.histories].reverse()

  const statusMutation = useMutation({
    mutationFn: (action: 'complete' | 'no-show' | 'cancel') =>
      action === 'complete'
        ? completeAppointment(appointment.id)
        : action === 'no-show'
          ? markAppointmentNoShow(appointment.id)
          : cancelAppointment(appointment.id, reason.trim()),
    onSuccess: (message) => {
      toast.success(message)
      setConfirming(null)
      // iptal edilen randevu takvimden kalkar, panel de kendiliğinden kapanır
      void queryClient.invalidateQueries({ queryKey: appointmentsQueryKey })
    },
  })

  return (
    <aside
      ref={ref}
      aria-label="Randevu detayı"
      className="flex w-full flex-col gap-5 rounded-card border border-line bg-surface p-5.5 lg:w-90 lg:shrink-0"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 flex-col gap-1">
          <span className="text-xs font-medium text-muted">
            {appointment.appointmentNumber} · {appointment.source}
          </span>
          <h2 className="truncate text-2xl font-bold">{appointment.customerFullName}</h2>
          {appointment.customerPhoneNumber && (
            <a
              href={`tel:${appointment.customerPhoneNumber}`}
              className="text-[13px] text-muted hover:text-ink"
            >
              {formatPhone(appointment.customerPhoneNumber)}
            </a>
          )}
        </div>
        <button
          type="button"
          aria-label="Kapat"
          onClick={onClose}
          className="flex size-9 shrink-0 cursor-pointer items-center justify-center rounded-full border border-line hover:bg-sand-soft"
        >
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden
          >
            <path d="M18 6 6 18M6 6l12 12" />
          </svg>
        </button>
      </div>

      {appointment.customerNoShowCount >= NO_SHOW_WARNING && (
        <p
          role="note"
          className="rounded-xl border border-clay/30 bg-clay-soft/50 px-3.5 py-2.5 text-[13px] text-clay-dark"
        >
          Bu müşteri {appointment.customerNoShowCount} randevuya gelmedi. Randevudan önce aramak iyi
          olabilir.
        </p>
      )}

      <div className="grid grid-cols-2 gap-2.5">
        <div
          className={`col-span-2 flex flex-col gap-0.5 rounded-xl p-3 ${staffColorClasses[appointment.staffColor]}`}
        >
          <span className="text-xs font-medium first-letter:uppercase">
            {day}, {formatClock(appointment.startAt)} – {formatClock(appointment.endAt)}
          </span>
          <span className="text-[15px] font-bold">
            {services} · {appointment.staffFullName}
          </span>
        </div>
        <div className="flex flex-col gap-0.5 rounded-xl bg-sand p-3">
          <span className="text-xs text-muted">Ücret</span>
          <span className="text-[15px] font-bold">{formatPrice(appointment.total)}</span>
        </div>
        <div className="flex flex-col gap-0.5 rounded-xl bg-sand p-3">
          <span className="text-xs text-muted">Gelmeme</span>
          <span className="text-[15px] font-bold">{appointment.customerNoShowCount} kez</span>
        </div>
      </div>

      <div className="flex items-center gap-2">
        <span className={sectionTitle}>Durum</span>
        <span
          className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${statusStyles[appointment.status]}`}
        >
          {appointment.status}
        </span>
      </div>

      {appointment.note && (
        <div className="flex flex-col gap-1.5">
          <span className={sectionTitle}>Randevu notu</span>
          <p className="text-sm leading-relaxed">{appointment.note}</p>
        </div>
      )}

      <div className="flex flex-col gap-2.5">
        <span className={sectionTitle}>Geçmiş</span>
        <ol className="flex flex-col gap-2.5">
          {histories.map((history) => (
            <li key={`${history.createdAt}-${history.title}`} className="flex gap-2.5 text-[13px]">
              <span className="mt-1.5 size-2 shrink-0 rounded-full bg-line" />
              <span className="flex flex-col">
                <span className="font-semibold">{history.title}</span>
                <span className="text-muted">
                  {formatDateTime(history.createdAt)} · {history.description}
                </span>
              </span>
            </li>
          ))}
        </ol>
      </div>

      {isOpen && !canManage && (
        <p className="rounded-xl bg-sand px-3.5 py-2.5 text-[13px] text-muted">
          Bu randevu {appointment.staffFullName} adına; sadece görüntüleyebilirsin.
        </p>
      )}

      {isOpen &&
        canManage &&
        (confirming === 'no-show' ? (
          <div className="flex flex-col gap-2.5">
            <p className="text-sm">
              <strong>{appointment.customerFullName}</strong> gelmedi olarak işaretlensin mi?
              Gelmeme sayısı {appointment.customerNoShowCount + 1} olacak, bu işlem geri alınamaz.
            </p>
            <div className="grid grid-cols-2 gap-2">
              <button
                type="button"
                onClick={() => setConfirming(null)}
                className="h-11 cursor-pointer rounded-full border border-line text-sm font-semibold hover:bg-sand-soft"
              >
                Vazgeç
              </button>
              <button
                type="button"
                disabled={statusMutation.isPending}
                onClick={() => statusMutation.mutate('no-show')}
                className="h-11 cursor-pointer rounded-full bg-clay text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
              >
                Gelmedi
              </button>
            </div>
          </div>
        ) : confirming === 'cancel' ? (
          <div className="flex flex-col gap-2.5">
            <label htmlFor={reasonId} className="text-[13px] font-semibold">
              İptal nedeni
            </label>
            <textarea
              id={reasonId}
              rows={2}
              maxLength={500}
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              placeholder="İsteğe bağlı, örn. müşteri aradı"
              className="w-full resize-y rounded-xl border border-[#dcd0c1] bg-surface px-3.5 py-3 text-sm outline-none placeholder:text-muted/60 focus:border-clay focus:ring-3 focus:ring-clay-soft"
            />
            <div className="grid grid-cols-2 gap-2">
              <button
                type="button"
                onClick={() => setConfirming(null)}
                className="h-11 cursor-pointer rounded-full border border-line text-sm font-semibold hover:bg-sand-soft"
              >
                Vazgeç
              </button>
              <button
                type="button"
                disabled={statusMutation.isPending}
                onClick={() => statusMutation.mutate('cancel')}
                className="h-11 cursor-pointer rounded-full bg-clay text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
              >
                İptal et
              </button>
            </div>
          </div>
        ) : (
          <div className="flex flex-col gap-2">
            <button
              type="button"
              disabled={!hasStarted || statusMutation.isPending}
              onClick={() => statusMutation.mutate('complete')}
              className="flex h-11.5 cursor-pointer items-center justify-center gap-2 rounded-full bg-ink text-sm font-semibold text-white disabled:cursor-default disabled:opacity-50"
            >
              <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.4"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden
              >
                <path d="M20 6 9 17l-5-5" />
              </svg>
              Tamamlandı
            </button>
            <div className="grid grid-cols-2 gap-2">
              <button
                type="button"
                onClick={() => setRescheduling(true)}
                className="h-11 cursor-pointer rounded-full border border-line text-sm font-semibold hover:bg-sand-soft"
              >
                Yeniden planla
              </button>
              <button
                type="button"
                disabled={!hasStarted || statusMutation.isPending}
                onClick={() => setConfirming('no-show')}
                className="h-11 cursor-pointer rounded-full border border-line text-sm font-semibold hover:bg-sand-soft disabled:cursor-default disabled:opacity-50"
              >
                Gelmedi
              </button>
            </div>
            {!hasStarted && (
              <p className="text-center text-xs text-muted">
                Tamamlandı ve Gelmedi randevu saati gelince işaretlenebilir.
              </p>
            )}
            <button
              type="button"
              onClick={() => setConfirming('cancel')}
              className="h-11 cursor-pointer rounded-full border border-line text-sm font-semibold text-clay-dark hover:bg-clay-soft/50"
            >
              Randevuyu iptal et
            </button>
          </div>
        ))}

      {rescheduling && (
        <RescheduleDialog
          appointment={appointment}
          onClose={() => setRescheduling(false)}
          onRescheduled={(date) => {
            setRescheduling(false)
            onRescheduled(date)
          }}
        />
      )}
    </aside>
  )
}
