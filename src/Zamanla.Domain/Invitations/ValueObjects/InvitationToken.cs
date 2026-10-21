using System.Security.Cryptography;
using System.Text;

namespace Zamanla.Domain.Invitations.ValueObjects;

// davet linkindeki anahtar; veritabanında SHA256 özeti tutulur
public sealed record InvitationToken(string Value)
{
    public static InvitationToken FromPlainText(string plainText)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return new(Convert.ToHexString(hash));
    }
}
