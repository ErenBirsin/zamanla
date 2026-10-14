export interface SalonService {
  id: string
  name: string
  category: string
  durationMinutes: number
  bufferMinutes: number
  price: number
  isActive: boolean
  createdAt: string
  createdFullName: string | null
  updatedAt: string | null
  updatedFullName: string | null
}
