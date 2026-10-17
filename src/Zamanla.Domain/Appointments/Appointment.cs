using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Shared;

namespace Zamanla.Domain.Appointments;

public sealed class Appointment : Entity, ITenantEntity
{
    private readonly List<AppointmentSalonService> _salonServices = new();
    private readonly List<AppointmentHistory> _histories = new();

    private Appointment() { }

    public Appointment(
        Guid customerId,
        Guid staffId,
        IEnumerable<AppointmentSalonService> salonServices,
        DateTimeOffset startAt,
        Source source,
        Note? note,
        DateTimeOffset now)
    {
        SetAppointmentNumber(now);
        SetCustomerId(customerId);
        SetStaffId(staffId);
        // süre hizmetlerden hesaplandığı için SetTime'dan önce olmalı
        SetSalonServices(salonServices);
        SetTime(startAt);
        SetSource(source);
        SetNote(note);
        SetStatus(Status.Confirmed);
        AddHistory(new AppointmentHistory(
            "Randevu oluşturuldu",
            source == Source.Online ? "Online randevu sayfasından alındı" : "Panelden oluşturuldu",
            now));
    }

    // kayıt sırasında TenantInterceptor doldurur
    public Guid TenantId { get; private set; }
    public AppointmentNumber AppointmentNumber { get; private set; } = default!;
    public Guid CustomerId { get; private set; }
    public Guid StaffId { get; private set; }
    public IReadOnlyCollection<AppointmentSalonService> SalonServices => _salonServices;
    // müşteriye gösterilen başlangıç ve bitiş
    public DateTimeRange Time { get; private set; } = default!;
    // bitiş + hazırlık payı; personel bu saate kadar dolu sayılır
    public DateTimeOffset BlockedUntil { get; private set; }
    public Price Total { get; private set; } = default!;
    public Status Status { get; private set; } = default!;
    public Source Source { get; private set; } = default!;
    public Note? Note { get; private set; }
    public IReadOnlyCollection<AppointmentHistory> Histories => _histories;

    // tamamlanan, gelinmeyen ya da iptal edilen randevu tekrar değiştirilemez
    public bool IsFinished => Status != Status.Confirmed;

    #region Behaviors
    public void SetCustomerId(Guid customerId)
    {
        CustomerId = customerId;
    }

    public void SetStaffId(Guid staffId)
    {
        StaffId = staffId;
    }

    public void SetSalonServices(IEnumerable<AppointmentSalonService> salonServices)
    {
        _salonServices.Clear();
        _salonServices.AddRange(salonServices);
        Total = new Price(_salonServices.Sum(s => s.Price));
    }

    // hizmetler arka arkaya yapılır; hazırlık payı sonda bir kez eklenir (en uzunu)
    public void SetTime(DateTimeOffset startAt)
    {
        var start = startAt.ToUniversalTime();
        var end = start.AddMinutes(_salonServices.Sum(s => s.Duration));
        Time = new DateTimeRange(start, end);
        BlockedUntil = end.AddMinutes(_salonServices.Max(s => s.Buffer));
    }

    public void SetSource(Source source)
    {
        Source = source;
    }

    public void SetNote(Note? note)
    {
        Note = note;
    }

    public void Reschedule(Guid staffId, DateTimeOffset startAt, DateTimeOffset now)
    {
        var description = staffId == StaffId
            ? "Randevu yeni saate taşındı"
            : "Randevu başka bir personele taşındı";

        SetStaffId(staffId);
        SetTime(startAt);
        AddHistory(new AppointmentHistory("Randevu saati değiştirildi", description, now));
    }

    public void Complete(DateTimeOffset now)
    {
        SetStatus(Status.Completed);
        AddHistory(new AppointmentHistory("Randevu tamamlandı", "Hizmet verildi", now));
    }

    public void MarkAsNoShow(DateTimeOffset now)
    {
        SetStatus(Status.NoShow);
        AddHistory(new AppointmentHistory("Müşteri gelmedi", "Müşteri randevuya gelmedi", now));
    }

    public void Cancel(string description, DateTimeOffset now)
    {
        SetStatus(Status.Cancelled);
        AddHistory(new AppointmentHistory("Randevu iptal edildi", description, now));
    }

    private void SetStatus(Status status)
    {
        Status = status;
    }

    private void AddHistory(AppointmentHistory history)
    {
        _histories.Add(history);
    }

    private void SetAppointmentNumber(DateTimeOffset now)
    {
        Random random = new();
        var num = string.Concat(Enumerable.Range(0, 8).Select(_ => random.Next(10)));
        AppointmentNumber = new AppointmentNumber($"RDV-{now.Year}-{num}");
    }
    #endregion
}
