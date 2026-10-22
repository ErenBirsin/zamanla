using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Domain.RefreshTokens;
using Zamanla.Domain.Users;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.Application.Auth;

// giriş yapmış kullanıcı kendi şifresini değiştirir; diğer cihazlardaki oturumlar kapanır, bu cihaz yeni token alır
public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest<Result<AuthTokens>>;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(p => p.CurrentPassword).NotEmpty().WithMessage("Mevcut şifrenizi girin");
        RuleFor(p => p.NewPassword)
            .NotEmpty().WithMessage("Geçerli bir yeni şifre girin")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalı")
            .NotEqual(p => p.CurrentPassword).WithMessage("Yeni şifre mevcut şifreyle aynı olamaz");
    }
}

internal sealed class ChangePasswordCommandHandler(
    IUserContext userContext,
    IAppUserRepository appUserRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IJwtProvider jwtProvider) : IRequestHandler<ChangePasswordCommand, Result<AuthTokens>>
{
    public async Task<Result<AuthTokens>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.GetUserId();
        AppUser? user = userId is null
            ? null
            : await appUserRepository.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<AuthTokens>.Failure(403, "Hesabınız pasif durumda");
        }

        if (!user.VerifyPassword(request.CurrentPassword))
        {
            return Result<AuthTokens>.Failure(400, "Mevcut şifre yanlış");
        }

        Password password = new(request.NewPassword);
        user.SetPassword(password);
        appUserRepository.Update(user);

        var refreshTokens = await refreshTokenRepository
            .WhereWithTracking(t => t.UserId == user.Id)
            .ToListAsync(cancellationToken);
        refreshTokenRepository.DeleteRange(refreshTokens);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await jwtProvider.CreateTokensAsync(user, cancellationToken);
    }
}
