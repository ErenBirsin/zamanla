import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { toast } from 'sonner'
import { usePermissions } from '@/features/auth/usePermissions'
import { appointmentsQueryKey } from '@/features/calendar/useDayAppointments'
import { PERMISSIONS } from '@/shared/auth/permissions'
import { describeTimeOff } from './format'
import { TimeOffFormDialog } from './TimeOffFormDialog'
import { deleteTimeOff } from './timeOffsApi'
import type { TimeOff } from './types'
import { timeOffsQueryKey, useTimeOffs } from './useTimeOffs'
import { confirmAction } from '@/shared/ui/confirm'
import { ListSkeleton } from '@/shared/ui/Skeleton'
import { QueryError } from '@/shared/ui/QueryError'
import { ActionButton } from '@/shared/ui/ActionButton'

export function TimeOffsPage() {
  const { data: timeOffs = [], isPending, isError, error, refetch } = useTimeOffs()
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const canManage = usePermissions().can(PERMISSIONS.timeOffsManage)

  const deleteMutation = useMutation({
    mutationFn: deleteTimeOff,
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: timeOffsQueryKey })
      void queryClient.invalidateQueries({ queryKey: appointmentsQueryKey })
    },
  })

  const remove = (timeOff: TimeOff) => {
    const { date } = describeTimeOff(timeOff)
    const who = timeOff.staffFullName ?? 'Salonun kapalı günü'
    confirmAction({
      title: 'İzin silinsin mi?',
      message: `${who} · ${date}. Bu saatler yeniden randevuya açılır.`,
      confirmLabel: 'Sil',
      onConfirm: () => deleteMutation.mutate(timeOff.id),
    })
  }

  return (
    <>
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div className="flex flex-col gap-1.5">
          <h1 className="text-3xl font-bold">İzinler</h1>
          <p className="text-muted">
            Personel izinleri ve salonun kapalı olduğu günler. Bu saatlerde randevu alınamaz.
          </p>
        </div>
        {canManage && (
          <button
            type="button"
            onClick={() => setFormOpen(true)}
            className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark"
          >
            İzin ekle
          </button>
        )}
      </header>

      {isPending ? (
        <ListSkeleton />
      ) : isError ? (
        <QueryError error={error} onRetry={() => void refetch()} />
      ) : timeOffs.length === 0 ? (
        <div className="flex max-w-xl flex-col items-start gap-2 rounded-card border border-dashed border-line p-6">
          <h2 className="text-lg font-bold">Yaklaşan izin yok</h2>
          <p className="text-sm text-muted">
            Personelin izinli olduğu saatleri ya da bayram gibi salonun kapalı olduğu günleri ekle;
            randevu sayfasında o saatler görünmez.
          </p>
        </div>
      ) : (
        <div className="relative overflow-x-auto rounded-card border border-line bg-surface">
          <table className="w-full min-w-160 text-left text-sm">
            <thead className="border-b border-line text-xs font-semibold tracking-wide text-muted">
              <tr>
                <th className="px-5 py-3">KİM</th>
                <th className="px-5 py-3">TARİH</th>
                <th className="px-5 py-3">SAAT</th>
                <th className="px-5 py-3">AÇIKLAMA</th>
                <th className="px-5 py-3">
                  <span className="sr-only">İşlemler</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {timeOffs.map((timeOff) => {
                const { date, time } = describeTimeOff(timeOff)
                return (
                  <tr key={timeOff.id} className="border-b border-line-soft last:border-b-0">
                    <td className="px-5 py-3.5">
                      {timeOff.staffId ? (
                        <span className="font-semibold">{timeOff.staffFullName}</span>
                      ) : (
                        <span className="rounded-full bg-clay-soft px-2.5 py-1 text-xs font-semibold text-clay-dark">
                          Salon kapalı
                        </span>
                      )}
                    </td>
                    <td className="px-5 py-3.5 text-ink-soft">{date}</td>
                    <td className="px-5 py-3.5 text-ink-soft tabular-nums">{time}</td>
                    <td className="px-5 py-3.5 text-ink-soft">{timeOff.note ?? '—'}</td>
                    <td className="px-5 py-3.5">
                      {canManage && (
                        <div className="flex justify-end">
                          <ActionButton icon="delete" onClick={() => remove(timeOff)}>
                            Sil
                          </ActionButton>
                        </div>
                      )}
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      <TimeOffFormDialog open={formOpen} onClose={() => setFormOpen(false)} />
    </>
  )
}
