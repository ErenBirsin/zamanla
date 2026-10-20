import type { AppointmentStatus } from '@/features/calendar/types'
import type { Address } from '@/features/settings/types'
import type { StaffColor } from '@/features/staff/staffOptions'
import type { WorkingHour } from '@/features/staff/types'

export interface PublicService {
  id: string
  name: string
  category: string
  durationMinutes: number
  price: number
}

export interface PublicStaff {
  id: string
  firstName: string
  lastName: string
  color: StaffColor
  salonServiceIds: string[]
  workingHours: WorkingHour[]
}

export interface PublicSalon {
  name: string
  address: Address | null
  phoneNumber: string | null
  bookingDaysAhead: number
  services: PublicService[]
  staff: PublicStaff[]
  cancelHoursBefore: number
}

export interface PublicBookingBody {
  firstName: string
  lastName: string
  phoneNumber: string
  email: string | null // boşsa onay ve hatırlatma sadece SMS ile gider
  staffId: string
  salonServiceIds: string[]
  startAt: string
  note: string
}

export interface PublicAppointmentCreated {
  appointmentNumber: string
  // yönetim linki için; sunucu bunu bir daha göstermez
  manageToken: string
  startAt: string
  endAt: string
}

export interface PublicAppointment {
  appointmentNumber: string
  customerFullName: string
  status: AppointmentStatus
  startAt: string
  endAt: string
  staffId: string
  staffFullName: string
  services: { id: string; name: string }[]
  total: number
  canCancel: boolean
  canReschedule: boolean
  cancelDeadline: string
}
