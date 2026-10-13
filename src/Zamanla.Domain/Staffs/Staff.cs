using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs.ValueObjects;

namespace Zamanla.Domain.Staffs;

public sealed class Staff : SoftDeletableEntity, ITenantEntity
{
    private readonly List<WorkingHour> _workingHours = new();
    private readonly List<StaffSalonService> _salonServices = new();

    private Staff() { }

    public Staff(
        FirstName firstName,
        LastName lastName,
        Color color,
        IEnumerable<WorkingHour> workingHours,
        IEnumerable<StaffSalonService> salonServices)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetColor(color);
        SetWorkingHours(workingHours);
        SetSalonServices(salonServices);
        Activate();
    }

    // kayıt sırasında TenantInterceptor doldurur
    public Guid TenantId { get; private set; }
    public FirstName FirstName { get; private set; } = default!;
    public LastName LastName { get; private set; } = default!;
    // takvimde personelin randevu rengi
    public Color Color { get; private set; } = default!;
    public bool IsActive { get; private set; }
    // aynı güne birden fazla aralık girilebilir (örneğin öğle arası)
    public IReadOnlyCollection<WorkingHour> WorkingHours => _workingHours;
    public IReadOnlyCollection<StaffSalonService> SalonServices => _salonServices;

    public string FullName => $"{FirstName.Value} {LastName.Value}";

    #region Behaviors
    public void SetFirstName(FirstName firstName)
    {
        FirstName = firstName;
    }

    public void SetLastName(LastName lastName)
    {
        LastName = lastName;
    }

    public void SetColor(Color color)
    {
        Color = color;
    }

    public void SetWorkingHours(IEnumerable<WorkingHour> workingHours)
    {
        _workingHours.Clear();
        _workingHours.AddRange(workingHours);
    }

    public void SetSalonServices(IEnumerable<StaffSalonService> salonServices)
    {
        _salonServices.Clear();
        _salonServices.AddRange(salonServices);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
    #endregion
}
