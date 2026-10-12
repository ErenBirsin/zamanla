using Zamanla.Application.Services;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Zamanla.Application.Auth;
public sealed record CurrentUserGetQuery : IRequest<Result<CurrentUserDto>>;
public sealed record CurrentUserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    Guid TenantId,
    string TenantName,
    string TenantSlug);

internal sealed class CurrentUserGetQueryHandler(
    IUserContext userContext,
    IAppUserRepository appUserRepository,
    ITenantRepository tenantRepository) : IRequestHandler<CurrentUserGetQuery, Result<CurrentUserDto>>
{
    public async Task<Result<CurrentUserDto>> Handle(CurrentUserGetQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.GetUserId();

        AppUser? user = userId is null
            ? null
            : await appUserRepository.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken, isTrackingActive: false);
        if (user is null)
        {
            return Result<CurrentUserDto>.Failure(404, "Kullanıcı bulunamadı");
        }

        Tenant? tenant = await tenantRepository.FirstOrDefaultAsync(t => t.Id == user.TenantId, cancellationToken, isTrackingActive: false);
        if (tenant is null)
        {
            return Result<CurrentUserDto>.Failure(404, "İşletme bulunamadı");
        }

        return new CurrentUserDto(
            user.Id,
            user.FirstName.Value,
            user.LastName.Value,
            user.Email.Value,
            user.Role.ToString(),
            tenant.Id,
            tenant.Name.Value,
            tenant.Slug.Value);
    }
}
