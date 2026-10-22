using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Users;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.Application.Auth;

public sealed record CheckForgotPasswordCodeCommand(
    string ForgotPasswordCode) : IRequest<Result<bool>>;

internal sealed class CheckForgotPasswordCodeCommandHandler(
    IAppUserRepository appUserRepository,
    TimeProvider timeProvider) : IRequestHandler<CheckForgotPasswordCodeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CheckForgotPasswordCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await appUserRepository.GetByForgotPasswordCodeIgnoringTenantAsync(
            ForgotPasswordCode.FromPlainText(request.ForgotPasswordCode), cancellationToken);
        if (user is null || !user.IsActive || !user.IsForgotPasswordCodeValid(timeProvider.GetUtcNow()))
        {
            return Result<bool>.Failure(404, "Şifre sıfırlama linki geçersiz, kullanılmış ya da süresi dolmuş. Yeni bir link isteyin");
        }

        return true;
    }
}
