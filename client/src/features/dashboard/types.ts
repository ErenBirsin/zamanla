export interface DashboardToday {
  appointmentCount: number
  expectedRevenue: number | null
  completedCount: number
  noShowCount: number
  cancelledCount: number
}

export interface DashboardUpcoming {
  id: string
  startAt: string
  endAt: string
  customerFullName: string
  staffFullName: string
  serviceNames: string[]
  isOnline: boolean
}

export interface DashboardDay {
  // "2026-10-21", salonun gününe göre
  date: string
  appointmentCount: number
  revenue: number | null
}

export interface Dashboard {
  today: DashboardToday
  upcoming: DashboardUpcoming[]
  lastSevenDays: DashboardDay[]
}
