using FluentValidation;
using GenericRepository;
using Zamanla.Domain.RefreshTokens;
using Zamanla.Domain.RefreshTokens.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace Zamanla.Application.Auth;

public sealed record LogoutCommand(
    string RefreshToken) : IRequest<Result<string>>;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(p => p.RefreshToken).NotEmpty().WithMessage("Geçerli bir refresh token girin");
    }
}

internal sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = Token.FromPlainText(request.RefreshToken);
        RefreshToken? refreshToken = await refreshTokenRepository.FirstOrDefaultAsync(
            t => t.Token.Value == token.Value,
            cancellationToken);

        if (refreshToken is not null && refreshToken.IsActive.Value)
        {
            refreshTokenRepository.Delete(refreshToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return "Çıkış yapıldı";
    }
}
