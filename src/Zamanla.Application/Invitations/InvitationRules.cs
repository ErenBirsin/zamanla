using Zamanla.Domain.Invitations;

namespace Zamanla.Application.Invitations;

// davet sayfası ve kabul aynı kurallarla açılır
internal static class InvitationRules
{
    public static (int StatusCode, string Message)? Check(Invitation? invitation, DateTimeOffset now)
    {
        if (invitation is null || invitation.AcceptedAt is not null)
        {
            return (404, "Davet bulunamadı ya da daha önce kullanılmış");
        }

        if (invitation.IsExpired(now))
        {
            return (410, "Davetin süresi dolmuş, salon sahibinden yeni bir davet isteyin");
        }

        return null;
    }
}
