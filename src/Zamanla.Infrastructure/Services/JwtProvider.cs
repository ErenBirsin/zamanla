using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GenericRepository;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Zamanla.Application.Services;
using Zamanla.Domain.RefreshTokens;
using Zamanla.Domain.RefreshTokens.ValueObjects;
using Zamanla.Domain.Users;
using Zamanla.Infrastructure.Options;

namespace Zamanla.Infrastructure.Services;

internal sealed class JwtProvider(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IOptions<JwtOptions> options) : IJwtProvider
{
    public async Task<AuthTokens> CreateTokensAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimNames.TenantId, user.TenantId.ToString())
        ];

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(options.Value.SecretKey));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha512);

        var accessTokenExpires = now.AddMinutes(options.Value.AccessTokenMinutes);
        JwtSecurityToken securityToken = new(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: accessTokenExpires.UtcDateTime,
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(securityToken);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenExpires = now.AddDays(options.Value.RefreshTokenDays);

        RefreshToken newRefreshToken = new(Token.FromPlainText(refreshToken), user.Id, new(refreshTokenExpires));
        refreshTokenRepository.Add(newRefreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokens(accessToken, accessTokenExpires, refreshToken, refreshTokenExpires);
    }
}
