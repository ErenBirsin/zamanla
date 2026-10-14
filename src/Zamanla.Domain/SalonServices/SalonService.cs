using Zamanla.Domain.Abstractions;
using Zamanla.Domain.SalonServices.ValueObjects;
using Zamanla.Domain.Shared;

namespace Zamanla.Domain.SalonServices;

public sealed class SalonService : SoftDeletableEntity, ITenantEntity
{
    private SalonService() { }

    public SalonService(Name name, Category category, Duration duration, Duration buffer, Price price)
    {
        SetName(name);
        SetCategory(category);
        SetDuration(duration);
        SetBuffer(buffer);
        SetPrice(price);
        Activate();
    }

    // kayıt sırasında TenantInterceptor doldurur
    public Guid TenantId { get; private set; }
    public Name Name { get; private set; } = default!;
    public Category Category { get; private set; } = default!;
    public Duration Duration { get; private set; } = default!;
    // iki randevu arasındaki hazırlık/temizlik payı
    public Duration Buffer { get; private set; } = default!;
    public Price Price { get; private set; } = default!;
    public bool IsActive { get; private set; }

    #region Behaviors
    public void SetName(Name name)
    {
        Name = name;
    }

    public void SetCategory(Category category)
    {
        Category = category;
    }

    public void SetDuration(Duration duration)
    {
        Duration = duration;
    }

    public void SetBuffer(Duration buffer)
    {
        Buffer = buffer;
    }

    public void SetPrice(Price price)
    {
        Price = price;
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
