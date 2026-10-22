using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.RefreshTokens;
using Zamanla.Domain.Users;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.Application.Auth;

public sealed record ResetPasswordCommand(
    string ForgotPasswordCode,
    string NewPassword) : IRequest<Result<string>>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(p => p.NewPassword)
            .NotEmpty().WithMessage("Geçerli bir yeni şifre girin")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalı");
    }
}

internal sealed class ResetPasswordCommandHandler(
    IAppUserRepository appUserRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await appUserRepository.GetByForgotPasswordCodeIgnoringTenantAsync(
            ForgotPasswordCode.FromPlainText(request.ForgotPasswordCode), cancellationToken);
        if (user is null || !user.IsActive || !user.IsForgotPasswordCodeValid(timeProvider.GetUtcNow()))
        {
            return Result<string>.Failure(404, "Şifre sıfırlama linki geçersiz, kullanılmış ya da süresi dolmuş. Yeni bir link isteyin");
        }

        Password password = new(request.NewPassword);
        user.SetPassword(password);
        user.SetIsForgotPasswordCompleted(true);
        appUserRepository.Update(user);

        var refreshTokens = await refreshTokenRepository
            .WhereWithTracking(t => t.UserId == user.Id && t.IsActive.Value)
            .ToListAsync(cancellationToken);
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.SetIsActive(new(false));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Şifreniz değiştirildi. Yeni şifrenizle giriş yapabilirsiniz";
    }
}
