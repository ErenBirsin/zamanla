using FluentValidation;
using Zamanla.Application.Services;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Zamanla.Application.Auth;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthTokens>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(p => p.Email).NotEmpty().WithMessage("Geçerli bir mail adresi girin");
        RuleFor(p => p.Password).NotEmpty().WithMessage("Geçerli bir şifre girin");
    }
}

internal sealed class LoginCommandHandler(
    IAppUserRepository appUserRepository,
    ITenantRepository tenantRepository,
    IJwtProvider jwtProvider) : IRequestHandler<LoginCommand, Result<AuthTokens>>
{
    public async Task<Result<AuthTokens>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await appUserRepository.GetByEmailIgnoringTenantAsync(new Email(request.Email), cancellationToken);
        if (user is null || !user.VerifyPassword(request.Password))
        {
            return Result<AuthTokens>.Failure(401, "Mail adresi ya da şifre yanlış");
        }

        if (!user.IsActive)
        {
            return Result<AuthTokens>.Failure(403, "Hesabınız pasif durumda");
        }

        Tenant? tenant = await tenantRepository.FirstOrDefaultAsync(t => t.Id == user.TenantId, cancellationToken);
        if (tenant is null || !tenant.IsActive)
        {
            return Result<AuthTokens>.Failure(403, "İşletme hesabı aktif değil");
        }

        return await jwtProvider.CreateTokensAsync(user, cancellationToken);
    }
}
