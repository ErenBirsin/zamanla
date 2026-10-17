namespace Zamanla.Domain.Appointments.ValueObjects;

// süre ve fiyat randevu anında kopyalanır; hizmet sonradan değişse de randevu değişmez
public sealed record AppointmentSalonService(Guid SalonServiceId, int Duration, int Buffer, decimal Price);
