using Zamanla.Domain.Users;

namespace Zamanla.Application.Services;

public interface IJwtProvider
{
    Task<AuthTokens> CreateTokensAsync(AppUser user, CancellationToken cancellationToken = default);
}

public sealed record AuthTokens(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
