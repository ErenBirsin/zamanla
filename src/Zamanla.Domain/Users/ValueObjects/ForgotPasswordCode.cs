using System.Security.Cryptography;
using System.Text;

namespace Zamanla.Domain.Users.ValueObjects;
public sealed record ForgotPasswordCode(string Value)
{
    public static ForgotPasswordCode FromPlainText(string plainText)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return new(Convert.ToHexString(hash));
    }
}
