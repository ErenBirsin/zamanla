// tarayıcının kendi Notification tipiyle karışmasın diye AppNotification
export interface AppNotification {
  id: string
  // "Yeni randevu", "Randevu iptal edildi", "Randevu saati değişti"
  type: string
  appointmentId: string
  customerFullName: string
  appointmentStartAt: string
  isRead: boolean
  createdAt: string
}
