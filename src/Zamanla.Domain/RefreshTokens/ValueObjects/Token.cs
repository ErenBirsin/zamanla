using System.Security.Cryptography;
using System.Text;

namespace Zamanla.Domain.RefreshTokens.ValueObjects;

public sealed record Token(string Value)
{
    // veritabanında token'ın kendisi değil SHA256 özeti tutulur
    public static Token FromPlainText(string plainText)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return new(Convert.ToHexString(hash));
    }
}
