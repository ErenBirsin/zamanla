export interface Address {
  city: string
  district: string
  fullAddress: string
}

export interface BookingPolicy {
  maxDaysAhead: number
  cancelHoursBefore: number
  maxUpcomingPerPhone: number
}

export interface Settings {
  name: string
  slug: string
  address: Address | null
  phoneNumber: string | null
  bookingPolicy: BookingPolicy
}
