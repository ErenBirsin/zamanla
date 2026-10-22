using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Invitations;
using Zamanla.Domain.RefreshTokens;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Users;

namespace Zamanla.Application.Staffs;

// personel kaydı değişince panel hesabını da günceller; kaydetmez, SaveChanges'i çağıran yapar
public sealed class StaffAccountService(
    IAppUserRepository appUserRepository,
    IInvitationRepository invitationRepository,
    IRefreshTokenRepository refreshTokenRepository)
{
    // personel silinince ya da pasif olunca hesabı kapanır, bekleyen daveti geçersiz olur
    public async Task CloseForStaffAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        var user = await appUserRepository.FirstOrDefaultAsync(u => u.StaffId == staffId, cancellationToken);
        if (user is not null && user.IsActive)
        {
            user.Deactivate();
            appUserRepository.Update(user);
            await RevokeSessionsAsync(user.Id, cancellationToken);
        }

        var pending = await invitationRepository
            .WhereWithTracking(i => i.StaffId == staffId && i.AcceptedAt == null)
            .ToListAsync(cancellationToken);
        invitationRepository.DeleteRange(pending);
    }

    // açık oturumlar kapanır; elindeki access token en fazla 15 dk daha geçerli, yetkili işlemlerde hesap pasif görünür
    public async Task RevokeSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await refreshTokenRepository
            .WhereWithTracking(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
        refreshTokenRepository.DeleteRange(tokens);
    }

    // takvimdeki ad değişince panelde de güncel görünsün; değer nesneleri paylaşılmaz
    public async Task SyncNameAsync(Staff staff, CancellationToken cancellationToken = default)
    {
        var user = await appUserRepository.FirstOrDefaultAsync(u => u.StaffId == staff.Id, cancellationToken);
        if (user is null)
        {
            return;
        }

        user.SetFirstName(new FirstName(staff.FirstName.Value));
        user.SetLastName(new LastName(staff.LastName.Value));
        appUserRepository.Update(user);
    }
}
