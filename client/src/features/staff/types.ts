import type { StaffColor } from './staffOptions'

export interface WorkingHour {
  day: number
  startTime: string
  endTime: string
}

export interface Staff {
  id: string
  firstName: string
  lastName: string
  color: StaffColor
  isActive: boolean
  workingHours: WorkingHour[]
  salonServiceIds: string[]
  createdAt: string
  createdFullName: string | null
  updatedAt: string | null
  updatedFullName: string | null
}
