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
        Activate();
    }

    public Name Name { get; private set; } = default!;
    public Slug Slug { get; private set; } = default!;
    public TimeZoneId TimeZoneId { get; private set; } = default!;
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
