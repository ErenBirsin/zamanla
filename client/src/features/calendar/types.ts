import type { StaffColor } from '@/features/staff/staffOptions'

export type AppointmentStatus = 'Onaylandı' | 'Tamamlandı' | 'Gelmedi' | 'İptal Edildi'

export interface AppointmentService {
  salonServiceId: string
  name: string
  duration: number
  price: number
}

export interface AppointmentHistory {
  title: string
  description: string
  createdAt: string
}

export interface Appointment {
  id: string
  appointmentNumber: string
  customerId: string
  customerFullName: string
  customerPhoneNumber: string | null
  customerNoShowCount: number
  staffId: string
  staffFullName: string
  staffColor: StaffColor
  startAt: string
  endAt: string
  blockedUntil: string
  total: number
  status: AppointmentStatus
  source: 'Panel' | 'Online'
  note: string | null
  salonServices: AppointmentService[]
  histories: AppointmentHistory[]
}

export interface AvailabilitySlot {
  startAt: string
  staffId: string
  staffFullName: string
}

export interface AppointmentCreateBody {
  customerId: string
  staffId: string
  salonServiceIds: string[]
  startAt: string
  note: string
}
