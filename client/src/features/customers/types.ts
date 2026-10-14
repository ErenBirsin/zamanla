export interface Customer {
  id: string
  firstName: string
  lastName: string
  fullName: string
  phoneNumber: string // +905321234567 biçiminde
  email: string | null
  note: string | null
  createdAt: string
  createdFullName: string | null
  updatedAt: string | null
  updatedFullName: string | null
}
