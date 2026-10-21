import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { toast } from 'sonner'
import { appointmentsQueryKey } from '@/features/calendar/useDayAppointments'
import { notificationsQueryKey } from '@/features/notifications/useNotifications'
import { timeOffsQueryKey } from '@/features/time-offs/useTimeOffs'
import { getFreshAccessToken } from '@/shared/api/http'

// sunucudaki NotificationHub sabitleriyle aynı
const CALENDAR_CHANGED = 'calendarChanged'
const NOTIFICATION_CREATED = 'notificationCreated'

// panel açıkken sunucu "değişti" der, ilgili sorgular yeniden çekilir; veri hub'dan gelmez
export function useRealtimeUpdates() {
  const queryClient = useQueryClient()

  useEffect(() => {
    const refreshCalendar = () => {
      void queryClient.invalidateQueries({ queryKey: appointmentsQueryKey })
      void queryClient.invalidateQueries({ queryKey: timeOffsQueryKey })
    }

    const refreshNotifications = () => {
      void queryClient.invalidateQueries({ queryKey: notificationsQueryKey })
    }

    const connection = new HubConnectionBuilder()
      .withUrl('/api/hubs/notifications', { accessTokenFactory: getFreshAccessToken })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on(CALENDAR_CHANGED, refreshCalendar)
    connection.on(NOTIFICATION_CREATED, () => {
      refreshNotifications()
      toast('Yeni bir bildirim var')
    })
    // bağlantı koptuğu sırada kaçan değişiklikler için
    connection.onreconnected(() => {
      refreshCalendar()
      refreshNotifications()
    })

    // bağlanamazsa panel eskisi gibi çalışır, sadece canlı güncelleme olmaz
    connection.start().catch(() => undefined)

    return () => {
      void connection.stop()
    }
  }, [queryClient])
}
