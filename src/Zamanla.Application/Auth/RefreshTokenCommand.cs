using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Zamanla.Application.Services;
using Zamanla.Domain.RefreshTokens;
using Zamanla.Domain.RefreshTokens.ValueObjects;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Zamanla.Application.Auth;

public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<Result<AuthTokens>>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(p => p.RefreshToken).NotEmpty().WithMessage("Geçerli bir refresh token girin");
    }
}

internal sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAppUserRepository appUserRepository,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    IJwtProvider jwtProvider,
    TimeProvider timeProvider) : IRequestHandler<RefreshTokenCommand, Result<AuthTokens>>
{
    private const string SessionExpired = "Oturumunuzun süresi doldu, tekrar giriş yapın";

    public async Task<Result<AuthTokens>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var token = Token.FromPlainText(request.RefreshToken);
        RefreshToken? refreshToken = await refreshTokenRepository.FirstOrDefaultAsync(
            t => t.Token.Value == token.Value,
            cancellationToken);

        if (refreshToken is null)
        {
            return Result<AuthTokens>.Failure(401, SessionExpired);
        }

        if (!refreshToken.IsActive.Value)
        {
            // kullanılmış token tekrar geldi: çalınmış olabilir, tüm oturumlar kapatılır
            var activeTokens = await refreshTokenRepository
                .WhereWithTracking(t => t.UserId == refreshToken.UserId && t.IsActive.Value)
                .ToListAsync(cancellationToken);
            foreach (var activeToken in activeTokens)
            {
                activeToken.SetIsActive(new(false));
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthTokens>.Failure(401, SessionExpired);
        }

        if (refreshToken.ExpiresDate.Value <= timeProvider.GetUtcNow())
        {
            return Result<AuthTokens>.Failure(401, SessionExpired);
        }

        var user = await appUserRepository.GetByIdIgnoringTenantAsync(refreshToken.UserId, cancellationToken);
        Tenant? tenant = user is null
            ? null
            : await tenantRepository.FirstOrDefaultAsync(t => t.Id == user.TenantId, cancellationToken);

        if (user is null || !user.IsActive || tenant is null || !tenant.IsActive)
        {
            return Result<AuthTokens>.Failure(401, SessionExpired);
        }

        refreshToken.SetIsActive(new(false));
        return await jwtProvider.CreateTokensAsync(user, cancellationToken);
    }
}
