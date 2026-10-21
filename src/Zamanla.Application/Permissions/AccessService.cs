using Zamanla.Application.Behaviors;
using Zamanla.Application.Services;
using Zamanla.Domain.Users;

namespace Zamanla.Application.Permissions;

// etiketle anlatılamayan, kayda bağlı yetki kontrolleri; kullanıcı her çağrıda veritabanından okunur
public sealed class AccessService(
    IUserContext userContext,
    IAppUserRepository appUserRepository)
{
    public async Task<AppUser> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId();
        AppUser? user = userId is null
            ? null
            : await appUserRepository.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken, isTrackingActive: false);
        if (user is null || !user.IsActive)
        {
            throw new AuthorizationException("Hesabınız pasif durumda");
        }

        return user;
    }

    // personel kendi randevularını yönetir; başka personelinki için yetki gerekir
    public async Task EnsureCanManageAppointmentAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        if (userContext.GetUserId() is null)
        {
            return;
        }
        var user = await GetCurrentUserAsync(cancellationToken);
        if (user.StaffId == staffId || user.HasPermission(PermissionCatalog.AppointmentsManageAll))
        {
            return;
        }

        throw new AuthorizationException(
            $"Başka personelin randevusu için \"{PermissionCatalog.GetLabel(PermissionCatalog.AppointmentsManageAll)}\" yetkisi gerekiyor");
    }

    public async Task<bool> CanViewRevenueAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        return user.HasPermission(PermissionCatalog.RevenueView);
    }
}
