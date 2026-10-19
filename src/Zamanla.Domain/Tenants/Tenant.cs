using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Tenants.ValueObjects;

namespace Zamanla.Domain.Tenants;

public sealed class Tenant : SoftDeletableEntity
{
    private Tenant() { }

    public Tenant(Name name, Slug slug, TimeZoneId timeZoneId)
    {
        SetName(name);
        SetSlug(slug);
        SetTimeZoneId(timeZoneId);
        SetBookingPolicy(BookingPolicy.Default);
        Activate();
    }

    public Name Name { get; private set; } = default!;
    public Slug Slug { get; private set; } = default!;
    public TimeZoneId TimeZoneId { get; private set; } = default!;
    // kayıtta sorulmuyor, salon sahibi ayarlardan ekler
    public Address? Address { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public BookingPolicy BookingPolicy { get; private set; } = default!;
    public bool IsActive { get; private set; }

    #region Behaviors

    public void SetName(Name name)
    {
        Name = name;
    }

    public void SetSlug(Slug slug)
    {
        Slug = slug;
    }

    public void SetTimeZoneId(TimeZoneId timeZoneId)
    {
        TimeZoneId = timeZoneId;
    }

    public void SetAddress(Address? address)
    {
        Address = address;
    }

    public void SetPhoneNumber(PhoneNumber? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public void SetBookingPolicy(BookingPolicy bookingPolicy)
    {
        BookingPolicy = bookingPolicy;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Suspend()
    {
        IsActive = false;
    }
    #endregion
}
