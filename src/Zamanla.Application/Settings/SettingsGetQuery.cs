using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Tenants.ValueObjects;

namespace Zamanla.Application.Settings;

public sealed record SettingsGetQuery() : IRequest<Result<SettingsDto>>;

public sealed class SettingsDto
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public Address? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public BookingPolicy BookingPolicy { get; set; } = default!;
}

internal sealed class SettingsGetQueryHandler(
    ITenantRepository tenantRepository,
    ITenantContext tenantContext) : IRequestHandler<SettingsGetQuery, Result<SettingsDto>>
{
    public async Task<Result<SettingsDto>> Handle(SettingsGetQuery request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.GetTenantId();
        var res = await tenantRepository
            .Where(t => t.Id == tenantId)
            .Select(t => new SettingsDto
            {
                Name = t.Name.Value,
                Slug = t.Slug.Value,
                Address = t.Address != null
                    ? new Address(t.Address.City, t.Address.District, t.Address.FullAddress)
                    : null,
                PhoneNumber = t.PhoneNumber != null ? t.PhoneNumber.Value : null,
                BookingPolicy = new BookingPolicy(
                    t.BookingPolicy.MaxDaysAhead,
                    t.BookingPolicy.CancelHoursBefore,
                    t.BookingPolicy.MaxUpcomingPerPhone)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
        {
            return Result<SettingsDto>.Failure(404, "Salon bulunamadı");
        }

        return res;
    }
}
