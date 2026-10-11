using Microsoft.AspNetCore.Identity;

namespace Zamanla.Domain.Users.ValueObjects;

public sealed record Password
{
    private Password()
    {
    }

    public Password(string password)
    {
        var passwordHasher = new PasswordHasher<object>();

        PasswordHash = passwordHasher.HashPassword(null!, password);
    }

    public string PasswordHash { get; private set; } = default!;

    public bool Verify(string password)
    {
        var passwordHasher = new PasswordHasher<object>();

        var result = passwordHasher.VerifyHashedPassword(null!, PasswordHash, password);

        return result != PasswordVerificationResult.Failed;
    }
}
