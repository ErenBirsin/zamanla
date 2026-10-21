using TS.MediatR;
using TS.Result;
using Zamanla.Application.Permissions;
using Zamanla.Application.Services;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Users;

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
    string TenantSlug,
    Guid? StaffId,
    List<string> Permissions); // sahipte bütün yetkiler; client menüyü ve düğmeleri buna göre gösterir

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
            tenant.Slug.Value,
            user.StaffId,
            user.IsOwner
                ? PermissionCatalog.All.Select(p => p.Value).ToList()
                : user.Permissions.Select(p => p.Value).ToList());
    }
}
