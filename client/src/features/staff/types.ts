import type { StaffColor } from './staffOptions'
import type { Permission } from '@/shared/auth/permissions'

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

// backend'deki StaffAccountStatus değerleri
export type StaffAccountStatus =
  'Hesap yok' | 'Davet gönderildi' | 'Davetin süresi doldu' | 'Aktif' | 'Pasif'

export interface StaffAccount {
  staffId: string
  staffFullName: string
  staffIsActive: boolean
  status: StaffAccountStatus
  // hesap açıldıysa dolu
  userId: string | null
  // hesabın ya da bekleyen davetin adresi
  email: string | null
  permissions: Permission[]
  invitationExpiresAt: string | null
}

export interface PermissionDefinition {
  value: Permission
  label: string
}

export interface InvitationCreated {
  link: string
  emailSent: boolean
  expiresAt: string
}
