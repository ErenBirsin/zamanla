using Zamanla.Domain.Invitations;

namespace Zamanla.Application.Services;

public sealed record InvitationLink(string Url, bool EmailSent);

// e-posta gönderilemese de davet geçerli; link salon sahibine gösterilir, kendisi iletebilir
public interface IInvitationMailService
{
    Task<InvitationLink> SendAsync(Invitation invitation, string token, CancellationToken cancellationToken = default);
}
