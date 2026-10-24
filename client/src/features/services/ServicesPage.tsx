import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { toast } from 'sonner'
import { usePermissions } from '@/features/auth/usePermissions'
import { PERMISSIONS } from '@/shared/auth/permissions'
import { formatPrice } from '@/shared/ui/format'
import { ServiceFormDialog } from './ServiceFormDialog'
import { deleteService } from './servicesApi'
import type { SalonService } from './types'
import { servicesQueryKey, useServices } from './useServices'
import { confirmAction } from '@/shared/ui/confirm'

export function ServicesPage() {
  const { data: services = [], isPending } = useServices()
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<SalonService | null>(null)
  const canManage = usePermissions().can(PERMISSIONS.servicesManage)

  const categories = [...new Set(services.map((service) => service.category))]

  const deleteMutation = useMutation({
    mutationFn: deleteService,
    onSuccess: (message) => {
      toast.success(message)
      void queryClient.invalidateQueries({ queryKey: servicesQueryKey })
    },
  })

  const openCreate = () => {
    setEditing(null)
    setFormOpen(true)
  }

  const openEdit = (service: SalonService) => {
    setEditing(service)
    setFormOpen(true)
  }

  const remove = (service: SalonService) => {
    confirmAction({
      title: 'Hizmet silinsin mi?',
      message: `"${service.name}" hizmeti silinecek. Geçmiş randevularda adı görünmeye devam eder.`,
      confirmLabel: 'Sil',
      onConfirm: () => deleteMutation.mutate(service.id),
    })
  }

  return (
    <>
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div className="flex flex-col gap-1.5">
          <h1 className="text-3xl font-bold">Hizmetler</h1>
          <p className="text-muted">Müşterilerin randevu alırken seçeceği hizmetler.</p>
        </div>
        {canManage && (
          <button
            type="button"
            onClick={openCreate}
            className="h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark"
          >
            Hizmet ekle
          </button>
        )}
      </header>

      {isPending ? (
        <p className="text-sm text-muted">Hizmetler yükleniyor…</p>
      ) : services.length === 0 ? (
        <div className="flex max-w-xl flex-col items-start gap-2 rounded-card border border-dashed border-line p-6">
          <h2 className="text-lg font-bold">Henüz hizmet yok</h2>
          <p className="text-sm text-muted">
            Saç kesimi, sakal tıraşı gibi sunduğun hizmetleri ekle; süre ve fiyatları randevu
            sayfanda görünür.
          </p>
        </div>
      ) : (
        <div className="relative overflow-x-auto rounded-card border border-line bg-surface">
          <table className="w-full min-w-160 text-left text-sm">
            <thead className="border-b border-line text-xs font-semibold tracking-wide text-muted">
              <tr>
                <th className="px-5 py-3">HİZMET</th>
                <th className="px-5 py-3">KATEGORİ</th>
                <th className="px-5 py-3">SÜRE</th>
                <th className="px-5 py-3 text-right">FİYAT</th>
                <th className="px-5 py-3">DURUM</th>
                <th className="px-5 py-3">
                  <span className="sr-only">İşlemler</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {services.map((service) => (
                <tr key={service.id} className="border-b border-line-soft last:border-b-0">
                  <td className="px-5 py-3.5 font-semibold">{service.name}</td>
                  <td className="px-5 py-3.5 text-ink-soft">{service.category}</td>
                  <td className="px-5 py-3.5 text-ink-soft">
                    {service.durationMinutes} dk
                    {service.bufferMinutes > 0 && (
                      <span className="text-muted"> + {service.bufferMinutes} dk hazırlık</span>
                    )}
                  </td>
                  <td className="px-5 py-3.5 text-right font-semibold tabular-nums">
                    {formatPrice(service.price)}
                  </td>
                  <td className="px-5 py-3.5">
                    <span
                      className={`rounded-full px-2.5 py-1 text-xs font-semibold ${
                        service.isActive ? 'bg-sage-soft text-sage' : 'bg-line-soft text-muted'
                      }`}
                    >
                      {service.isActive ? 'Aktif' : 'Pasif'}
                    </span>
                  </td>
                  <td className="px-5 py-3.5">
                    {canManage && (
                      <div className="flex justify-end gap-1">
                        <button
                          type="button"
                          onClick={() => openEdit(service)}
                          className="cursor-pointer rounded-full px-3 py-1.5 text-xs font-semibold hover:bg-sand-soft"
                        >
                          Düzenle
                        </button>
                        <button
                          type="button"
                          onClick={() => remove(service)}
                          className="cursor-pointer rounded-full px-3 py-1.5 text-xs font-semibold text-clay-dark hover:bg-clay-soft"
                        >
                          Sil
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <ServiceFormDialog
        open={formOpen}
        service={editing}
        categories={categories}
        onClose={() => setFormOpen(false)}
      />
    </>
  )
}
