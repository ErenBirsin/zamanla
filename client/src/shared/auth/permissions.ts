export const PERMISSIONS = {
  appointmentsManageAll: 'appointments:manage-all',
  revenueView: 'revenue:view',
  customersDelete: 'customers:delete',
  servicesManage: 'services:manage',
  staffManage: 'staff:manage',
  timeOffsManage: 'timeoffs:manage',
  settingsManage: 'settings:manage',
} as const

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS]
