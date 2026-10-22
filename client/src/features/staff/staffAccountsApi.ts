import { http } from '@/shared/api/http'
import type { Permission } from '@/shared/auth/permissions'
import type { Result } from '@/shared/models/result'
import type { InvitationCreated, PermissionDefinition, StaffAccount } from './types'

export async function getStaffAccounts() {
  const { data } = await http.get<Result<StaffAccount[]>>('/staff/accounts')
  return data.data!
}

export async function getPermissionCatalog() {
  const { data } = await http.get<Result<PermissionDefinition[]>>('/permissions')
  return data.data!
}

export async function inviteStaff(staffId: string, email: string, permissions: Permission[]) {
  const { data } = await http.post<Result<InvitationCreated>>('/invitations', {
    staffId,
    email,
    permissions,
  })
  return data.data!
}

export async function withdrawInvitation(staffId: string) {
  const { data } = await http.delete<Result<string>>(`/invitations/${staffId}`)
  return data.data!
}

export async function updateAccountPermissions(userId: string, permissions: Permission[]) {
  const { data } = await http.put<Result<string>>('/staff/accounts/permissions', {
    userId,
    permissions,
  })
  return data.data!
}

export async function updateAccountStatus(userId: string, isActive: boolean) {
  const { data } = await http.put<Result<string>>('/staff/accounts/status', { userId, isActive })
  return data.data!
}
