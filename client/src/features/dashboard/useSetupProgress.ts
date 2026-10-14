import { useServices } from '@/features/services/useServices'
import { useStaff } from '@/features/staff/useStaff'

export interface SetupStep {
  title: string
  description: string
  done: boolean
  summary: string
  to: string
  action: string
}

// adımlar verinin kendisinden hesaplanır, ayrıca "kurulum bitti" bilgisi saklanmaz
export function useSetupProgress() {
  const services = useServices()
  const staff = useStaff()

  const activeServices = (services.data ?? []).filter((service) => service.isActive)
  const activeServiceIds = new Set(activeServices.map((service) => service.id))
  const staffList = staff.data ?? []
  // randevu alabilecek personel: aktif, en az bir aktif hizmeti ve çalışma saati var
  const readyStaff = staffList.filter(
    (member) =>
      member.isActive &&
      member.workingHours.length > 0 &&
      member.salonServiceIds.some((id) => activeServiceIds.has(id)),
  )

  const steps: SetupStep[] = [
    {
      title: 'Hizmetlerini ekle',
      description: 'Saç kesimi, sakal tıraşı gibi sunduğun hizmetleri süre ve fiyatıyla ekle.',
      done: activeServices.length > 0,
      summary: `${activeServices.length} aktif hizmet`,
      to: '/app/services',
      action: 'Hizmet ekle',
    },
    {
      title: 'Personelini ekle',
      description: 'Randevu alacak çalışanlarını ekle; tek başına çalışıyorsan kendini ekle.',
      done: staffList.length > 0,
      summary: `${staffList.length} personel`,
      to: '/app/staff',
      action: 'Personel ekle',
    },
    {
      title: 'Hizmet ve çalışma saatlerini ata',
      description: 'Her personelin verdiği hizmetleri ve haftalık çalışma saatlerini seç.',
      done: readyStaff.length > 0,
      summary: `${readyStaff.length} personel randevuya hazır`,
      to: '/app/staff',
      action: 'Personeli düzenle',
    },
  ]

  return {
    steps,
    completed: steps.filter((step) => step.done).length,
    isLoading: services.isPending || staff.isPending,
  }
}
