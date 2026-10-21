using Zamanla.Domain.Invitations;
using Zamanla.Domain.Invitations.ValueObjects;

namespace Zamanla.Application.Invitations;

public static class InvitationTokenExtensions
{
    // linkteki anahtar özetlenip aranır; salon, adresteki slug'dan zaten seçilmiştir
    public static async Task<Invitation?> GetByTokenAsync(
        this IInvitationRepository invitationRepository,
        string token,
        CancellationToken cancellationToken = default)
    {
        var hash = InvitationToken.FromPlainText(token).Value;
        return await invitationRepository.FirstOrDefaultAsync(i => i.Token.Value == hash, cancellationToken);
    }
}
