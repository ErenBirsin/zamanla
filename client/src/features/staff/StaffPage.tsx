import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { toast } from 'sonner'
import { useServices } from '@/features/services/useServices'
import { formatTime } from './schedule'
import { StaffFormDialog } from './StaffFormDialog'
import { deleteStaff } from './staffApi'
import { staffColorClasses, weekDays } from './staffOptions'
import type { Staff, WorkingHour } from './types'
import { staffQueryKey, useStaff } from './useStaff'

export function StaffPage() {
  const { data: staffList = [], isPending } = useStaff()
  const { data: services = [] } = useServices()
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<Staff | null>(null)

  const serviceNames = new Map(services.map((service) => [service.id, service.name]))

  const deleteMutation = useMutation({
    mutationFn: deleteStaff,
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: staffQueryKey })
    },
  })

  const openCreate = () => {
    setEditing(null)
    setFormOpen(true)
  }

  const openEdit = (staff: Staff) => {
    setEditing(staff)
    setFormOpen(true)
  }

  const remove = (staff: Staff) => {
    if (
      window.confirm(`"${staff.firstName} ${staff.lastName}" kaydını silmek istediğine emin misin?`)
    ) {
      deleteMutation.mutate(staff.id)
    }
  }

  return (
    <>
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div className="flex flex-col gap-1.5">
          <h1 className="text-3xl font-bold">Personel</h1>
          <p className="text-muted">
            Randevu alan çalışanlar, verdikleri hizmetler ve çalışma saatleri.
          </p>
        </div>
        <button
          type="button"
          onClick={openCreate}
          className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark"
        >
          Personel ekle
        </button>
      </header>

      {isPending ? (
        <p className="text-sm text-muted">Personel yükleniyor…</p>
      ) : staffList.length === 0 ? (
        <div className="flex max-w-xl flex-col items-start gap-2 rounded-card border border-dashed border-line p-6">
          <h2 className="text-lg font-bold">Henüz personel yok</h2>
          <p className="text-sm text-muted">
            Çalışanlarını ekle; hangi hizmetleri verdiklerini ve haftanın hangi saatlerinde
            çalıştıklarını belirle. Müşteriler bu saatlere göre randevu alır.
          </p>
        </div>
      ) : (
        <div className="relative overflow-x-auto rounded-card border border-line bg-surface">
          <table className="w-full min-w-200 text-left text-sm">
            <thead className="border-b border-line text-xs font-semibold tracking-wide text-muted">
              <tr>
                <th className="px-5 py-3">PERSONEL</th>
                <th className="px-5 py-3">HİZMETLER</th>
                <th className="px-5 py-3">ÇALIŞMA GÜNLERİ</th>
                <th className="px-5 py-3">DURUM</th>
                <th className="px-5 py-3">
                  <span className="sr-only">İşlemler</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {staffList.map((staff) => {
                // silinmiş hizmetler listede görünmez
                const names = staff.salonServiceIds
                  .map((id) => serviceNames.get(id))
                  .filter((name) => name !== undefined)

                return (
                  <tr key={staff.id} className="border-b border-line-soft last:border-b-0">
                    <td className="px-5 py-3.5">
                      <div className="flex items-center gap-2.5 font-semibold">
                        <span
                          aria-hidden
                          className={`size-3.5 shrink-0 rounded-full border border-line ${staffColorClasses[staff.color]}`}
                        />
                        {staff.firstName} {staff.lastName}
                      </div>
                    </td>
                    <td className="max-w-64 px-5 py-3.5 text-ink-soft">
                      {names.length > 0 ? (
                        <span className="line-clamp-2">{names.join(', ')}</span>
                      ) : (
                        <span className="text-muted">Hizmet seçilmedi</span>
                      )}
                    </td>
                    <td className="px-5 py-3.5">
                      <WorkingDays hours={staff.workingHours} />
                    </td>
                    <td className="px-5 py-3.5">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-semibold ${
                          staff.isActive ? 'bg-sage-soft text-sage' : 'bg-line-soft text-muted'
                        }`}
                      >
                        {staff.isActive ? 'Aktif' : 'Pasif'}
                      </span>
                    </td>
                    <td className="px-5 py-3.5">
                      <div className="flex justify-end gap-1">
                        <button
                          type="button"
                          onClick={() => openEdit(staff)}
                          className="cursor-pointer rounded-full px-3 py-1.5 text-xs font-semibold hover:bg-sand-soft"
                        >
                          Düzenle
                        </button>
                        <button
                          type="button"
                          onClick={() => remove(staff)}
                          className="cursor-pointer rounded-full px-3 py-1.5 text-xs font-semibold text-clay-dark hover:bg-clay-soft"
                        >
                          Sil
                        </button>
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      <StaffFormDialog open={formOpen} staff={editing} onClose={() => setFormOpen(false)} />
    </>
  )
}

// haftanın 7 günü; çalışılan günler dolu, üzerine gelince saatler görünür
function WorkingDays({ hours }: { hours: WorkingHour[] }) {
  return (
    <div className="flex gap-1">
      {weekDays.map(({ day, label, short }) => {
        const ranges = hours.filter((hour) => hour.day === day)
        const detail =
          ranges.length > 0
            ? `${label}: ${ranges.map((r) => `${formatTime(r.startTime)}–${formatTime(r.endTime)}`).join(', ')}`
            : `${label}: kapalı`

        return (
          <span
            key={day}
            title={detail}
            className={`w-9 rounded-md py-1 text-center text-[11px] font-semibold ${
              ranges.length > 0 ? 'bg-sage-soft text-sage' : 'bg-line-soft text-muted/60'
            }`}
          >
            <span aria-hidden>{short}</span>
            <span className="sr-only">{detail}</span>
          </span>
        )
      })}
    </div>
  )
}
