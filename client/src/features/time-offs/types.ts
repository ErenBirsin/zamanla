export interface TimeOff {
  id: string
  // boşsa bütün salon kapalı
  staffId: string | null
  staffFullName: string | null
  startAt: string
  endAt: string
  note: string | null
}

export interface TimeOffCreateBody {
  staffId: string | null
  startAt: string
  endAt: string
  note: string | null
}
