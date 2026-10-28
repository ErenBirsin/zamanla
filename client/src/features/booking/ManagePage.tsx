import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useLocation, useParams } from 'react-router'
import { toast } from 'sonner'
import { formatDateTime } from '@/features/calendar/time'
import { formatPrice } from '@/shared/ui/format'
import { cancelManagedAppointment } from './bookingApi'
import { formatAddress, formatSlot, minutesBetween } from './format'
import { downloadIcs } from './ics'
import { RescheduleSection } from './RescheduleSection'
import { SalonNotFound } from './SalonNotFound'
import { useManagedAppointment } from './useManagedAppointment'
import { publicQueryKey, usePublicSalon } from './usePublicSalon'
import { formatPhone } from '@/features/customers/phone'
import { SalonInfoCard } from '@/features/booking/SalonInfoCard'

const statusNotes: Record<string, string> = {
  'İptal Edildi': 'Bu randevu iptal edildi.',
  Tamamlandı: 'Bu randevu tamamlandı. Yeniden bekleriz!',
  Gelmedi: 'Bu randevuya gelinmedi olarak işaretlendi.',
}

const actionButton =
  'flex h-12 flex-1 cursor-pointer items-center justify-center gap-2 rounded-full border border-line bg-surface text-[15px] font-semibold hover:bg-sand-soft'

export function ManagePage() {
  const { slug = '', token = '' } = useParams()
  const location = useLocation()
  // randevu az önce alındıysa onay başlığı gösterilir; link sonradan açılınca normal görünüm
  const justBooked = (location.state as { justBooked?: boolean } | null)?.justBooked === true
  const queryClient = useQueryClient()
  const salon = usePublicSalon(slug)
  const appointment = useManagedAppointment(slug, token)
  // iptal geri alınamaz; önce onay istenir
  const [mode, setMode] = useState<'reschedule' | 'cancel' | null>(null)

  const cancelMutation = useMutation({
    mutationFn: () => cancelManagedAppointment(slug, token),
    onSuccess: (message) => {
      toast.success(message)
      setMode(null)
      void queryClient.invalidateQueries({ queryKey: publicQueryKey(slug) })
    },
  })

  if (salon.isPending || appointment.isPending) {
    return <div className="min-h-dvh bg-sand" />
  }

  if (salon.isError) {
    return <SalonNotFound error={salon.error} />
  }

  if (appointment.isError) {
    return (
      <SalonNotFound
        error={appointment.error}
        title="Randevu bulunamadı"
        message="Link eksik kopyalanmış olabilir. Randevu bilgin için salonu arayabilirsin."
      />
    )
  }

  const item = appointment.data
  const isOpen = item.status === 'Onaylandı'
  const services = item.services.map((service) => service.name).join(' + ')
  const duration = minutesBetween(item.startAt, item.endAt)
  const address = salon.data.address ? formatAddress(salon.data.address) : null
  const phone = salon.data.phoneNumber

  const copyLink = async () => {
    await navigator.clipboard.writeText(window.location.href)
    toast.success('Link kopyalandı')
  }

  const addToCalendar = () =>
    downloadIcs({
      uid: item.appointmentNumber,
      title: `${salon.data.name} randevusu`,
      description: `${services} · ${item.staffFullName}\nRandevunu değiştirmek ya da iptal etmek için: ${window.location.href}`,
      startAt: item.startAt,
      endAt: item.endAt,
      url: window.location.href,
      location: address ?? undefined,
    })

  return (
    <div className="lg:mx-auto lg:flex lg:max-w-5xl lg:items-start lg:gap-10 lg:px-8 lg:py-10">
      <SalonInfoCard salon={salon.data} />
      <div className="mx-auto flex min-h-dvh max-w-md flex-col gap-6 px-5 pt-10 pb-8 lg:mx-0 lg:min-h-0 lg:max-w-xl lg:flex-1 lg:pt-0">
        {justBooked && isOpen ? (
          <div className="flex flex-col items-center gap-3 text-center">
            <span className="flex size-18 items-center justify-center rounded-full bg-sage-soft">
              <span className="flex size-12 items-center justify-center rounded-full bg-sage">
                <svg
                  width="22"
                  height="22"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="#fff"
                  strokeWidth="3"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  aria-hidden
                >
                  <path d="M20 6 9 17l-5-5" />
                </svg>
              </span>
            </span>
            <h1 className="text-[28px] leading-tight font-bold">
              Randevun hazır, {item.customerFullName.split(' ')[0]}!
            </h1>
            <p className="text-sm text-muted">
              Bu sayfanın linkini kaydet. Randevunu buradan görebilir, değiştirebilir ya da iptal
              edebilirsin.
            </p>
          </div>
        ) : (
          <div className="flex flex-col gap-1">
            <span className="text-[13px] text-muted">{item.customerFullName}</span>
            <h1 className="text-[28px] leading-tight font-bold">Randevun</h1>
          </div>
        )}

        {!isOpen && (
          <p
            role="note"
            className="rounded-xl border border-line bg-surface px-4 py-3 text-sm text-ink-soft"
          >
            {statusNotes[item.status]}
          </p>
        )}

        <div className="overflow-hidden rounded-[20px] border border-line bg-surface">
          <div className="flex flex-col gap-1 bg-ink px-4.5 py-4 text-sand">
            <span className="text-xs font-semibold text-[#cfc4b6]">
              {salon.data.name} · {item.appointmentNumber}
            </span>
            <span
              className={`font-display text-2xl font-bold ${isOpen ? '' : 'line-through opacity-70'}`}
            >
              {formatSlot(item.startAt)}
            </span>
          </div>
          <dl className="flex flex-col gap-2.5 px-4.5 py-4 text-sm">
            <Row label="Hizmet" value={`${services} · ${duration} dk`} />
            <Row label="Personel" value={item.staffFullName} />
            <Row label="Ücret" value={`${formatPrice(item.total)} · salonda`} />
            {address && <Row label="Adres" value={address} />}
          </dl>
        </div>

        <div className="flex gap-2.5">
          {isOpen && (
            <button type="button" onClick={addToCalendar} className={actionButton}>
              <svg
                width="17"
                height="17"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden
              >
                <rect x="3" y="4" width="18" height="18" rx="2" />
                <path d="M16 2v4M8 2v4M3 10h18M12 14v4M10 16h4" />
              </svg>
              Takvime ekle
            </button>
          )}
          <button type="button" onClick={() => void copyLink()} className={actionButton}>
            <svg
              width="17"
              height="17"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden
            >
              <path d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7" />
              <path d="M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7" />
            </svg>
            Linki kopyala
          </button>
        </div>

        {isOpen && (
          <div className="flex flex-col gap-2.5 rounded-2xl border border-line bg-sand-soft px-4 py-3.5 text-[13px] text-ink-soft">
            {item.canCancel ? (
              <p>
                Son ücretsiz iptal: <strong>{formatDateTime(item.cancelDeadline)}</strong>. O saate
                kadar bu sayfadan iptal edebilirsin.
              </p>
            ) : (
              <p>
                İptal için son saat geçti. Gelemeyeceksen lütfen salonu ara
                {phone && (
                  <>
                    :{' '}
                    <a href={`tel:${phone}`} className="font-semibold text-ink underline">
                      {formatPhone(phone)}
                    </a>
                  </>
                )}
                .
              </p>
            )}
            <p>Ödeme salonda yapılır. Linki kaybetmemek için Takvime ekle'yi kullanabilirsin.</p>
          </div>
        )}

        {isOpen && mode === 'reschedule' && (
          <RescheduleSection
            slug={slug}
            token={token}
            salon={salon.data}
            appointment={item}
            onClose={() => setMode(null)}
          />
        )}

        {isOpen && mode === 'cancel' && (
          <div className="flex flex-col gap-3 rounded-[20px] border border-clay/30 bg-clay-soft/50 p-4.5">
            <p className="text-sm">
              <strong>{formatSlot(item.startAt)}</strong> randevun iptal edilsin mi? Bu işlem geri
              alınamaz.
            </p>
            <div className="grid grid-cols-2 gap-2">
              <button
                type="button"
                onClick={() => setMode(null)}
                className="h-11 cursor-pointer rounded-full border border-line bg-surface text-sm font-semibold hover:bg-sand-soft"
              >
                Vazgeç
              </button>
              <button
                type="button"
                disabled={cancelMutation.isPending}
                onClick={() => cancelMutation.mutate()}
                className="h-11 cursor-pointer rounded-full bg-clay text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60"
              >
                İptal et
              </button>
            </div>
          </div>
        )}

        {isOpen && mode === null && (item.canReschedule || item.canCancel) && (
          <div className="flex flex-col gap-2">
            {item.canReschedule && (
              <button
                type="button"
                onClick={() => setMode('reschedule')}
                className="h-12 cursor-pointer rounded-full bg-ink text-[15px] font-semibold text-white"
              >
                Saati değiştir
              </button>
            )}
            {item.canCancel && (
              <button
                type="button"
                onClick={() => setMode('cancel')}
                className="h-11 cursor-pointer text-sm font-semibold text-clay-dark hover:text-clay"
              >
                Randevuyu iptal et
              </button>
            )}
          </div>
        )}

        <footer className="mt-auto pt-6 text-center text-xs text-muted">
          <strong className="font-display text-sm text-ink">zamanla</strong> ile alındı
        </footer>
      </div>
    </div>
  )
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-start justify-between gap-4">
      <dt className="text-muted">{label}</dt>
      <dd className="text-right font-semibold">{value}</dd>
    </div>
  )
}
