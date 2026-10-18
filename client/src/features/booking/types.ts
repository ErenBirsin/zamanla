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
  bookingDaysAhead: number
  services: PublicService[]
  staff: PublicStaff[]
}
