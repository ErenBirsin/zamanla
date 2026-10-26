import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router'
import { formatDateTime, toDateKey } from '@/features/calendar/time'
import { formatRelative } from './format'
import { markAllNotificationsRead, markNotificationRead } from './notificationsApi'
import type { AppNotification } from './types'
import { notificationsQueryKey, useNotifications, useUnreadCount } from './useNotifications'

function BellIcon() {
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
      <path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9" />
      <path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" />
    </svg>
  )
}

export function NotificationBell() {
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { data: unreadCount = 0 } = useUnreadCount()
  const { data: notifications = [], isPending } = useNotifications(open)

  // dışarı tıklayınca ya da Esc ile kapanır
  useEffect(() => {
    if (!open) return

    const onPointerDown = (event: PointerEvent) => {
      if (!containerRef.current?.contains(event.target as Node)) setOpen(false)
    }
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false)
    }
    document.addEventListener('pointerdown', onPointerDown)
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('pointerdown', onPointerDown)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [open])

  const refresh = () => {
    void queryClient.invalidateQueries({ queryKey: notificationsQueryKey })
  }
  const readMutation = useMutation({ mutationFn: markNotificationRead, onSuccess: refresh })
  const readAllMutation = useMutation({ mutationFn: markAllNotificationsRead, onSuccess: refresh })

  // bildirime tıklayınca okundu sayılır ve randevunun günü takvimde açılır
  const openNotification = (notification: AppNotification) => {
    if (!notification.isRead) readMutation.mutate(notification.id)
    setOpen(false)
    void navigate(`/app/calendar?date=${toDateKey(notification.appointmentStartAt)}`)
  }

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        aria-label={unreadCount > 0 ? `Bildirimler, ${unreadCount} okunmamış` : 'Bildirimler'}
        aria-expanded={open}
        onClick={() => setOpen((value) => !value)}
        className="relative flex size-10 cursor-pointer items-center justify-center rounded-full border border-line bg-surface hover:bg-line-soft"
      >
        <BellIcon />
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-clay px-1 text-[11px] font-bold text-white">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute top-12 right-0 z-20 flex w-80 max-w-[calc(100vw-2rem)] flex-col rounded-card border border-line bg-surface shadow-xl md:right-auto md:left-0">
          <div className="flex items-center justify-between border-b border-line px-4 py-3">
            <span className="text-sm font-bold">Bildirimler</span>
            {unreadCount > 0 && (
              <button
                type="button"
                onClick={() => readAllMutation.mutate()}
                className="cursor-pointer text-xs font-semibold text-clay-dark hover:underline"
              >
                Tümünü okundu say
              </button>
            )}
          </div>

          {isPending ? (
            <p className="px-4 py-6 text-sm text-muted">Bildirimler yükleniyor…</p>
          ) : notifications.length === 0 ? (
            <p className="px-4 py-6 text-sm text-muted">
              Henüz bildirim yok. Müşteriler online randevu alınca, iptal edince ya da saatini
              değiştirince burada görünür.
            </p>
          ) : (
            <ul className="max-h-96 overflow-y-auto">
              {notifications.map((notification) => (
                <li key={notification.id} className="border-b border-line-soft last:border-b-0">
                  <button
                    type="button"
                    onClick={() => openNotification(notification)}
                    className="flex w-full cursor-pointer gap-3 px-4 py-3 text-left hover:bg-sand-soft"
                  >
                    <span
                      aria-hidden
                      className={`mt-1.5 size-2 shrink-0 rounded-full ${notification.isRead ? '' : 'bg-clay'}`}
                    />
                    <span className="flex min-w-0 flex-col gap-0.5">
                      <span
                        className={`text-sm ${notification.isRead ? 'font-semibold text-ink-soft' : 'font-bold'}`}
                      >
                        {notification.type}
                      </span>
                      <span className="truncate text-[13px] text-ink-soft">
                        {notification.customerFullName} ·{' '}
                        {formatDateTime(notification.appointmentStartAt)}
                      </span>
                      <span className="text-xs text-muted">
                        {formatRelative(notification.createdAt)}
                      </span>
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  )
}
