using System.Text.RegularExpressions;

namespace Zamanla.Domain.Shared;

public sealed record PhoneNumber
{
    public PhoneNumber(string value)
    {
        Value = Normalize(value);
    }

    public string Value { get; private set; }

    public static string Normalize(string value)
    {
        var digits = new string(value.Where(char.IsAsciiDigit).ToArray());

        if (digits.Length == 12 && digits.StartsWith("90"))
        {
            digits = digits[2..];
        }
        else if (digits.Length == 11 && digits.StartsWith('0'))
        {
            digits = digits[1..];
        }

        return "+90" + digits;
    }

    public static bool IsValid(string value) => Regex.IsMatch(Normalize(value), @"^\+90[2-5]\d{9}$");
}
