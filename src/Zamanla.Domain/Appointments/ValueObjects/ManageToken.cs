using System.Security.Cryptography;
using System.Text;

namespace Zamanla.Domain.Appointments.ValueObjects;

// müşterinin randevusunu yönettiği linkteki token; veritabanında SHA256 özeti tutulur
public sealed record ManageToken(string Value)
{
    public static ManageToken FromPlainText(string plainText)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return new(Convert.ToHexString(hash));
    }
}
