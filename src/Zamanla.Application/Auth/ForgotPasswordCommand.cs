using System.Buffers.Text;
using System.Security.Cryptography;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Users;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.Application.Auth;

public sealed record ForgotPasswordCommand(
    string Email) : IRequest<Result<string>>;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Geçerli bir mail adresi girin")
            .EmailAddress().WithMessage("Geçerli bir mail adresi girin");
    }
}

internal sealed class ForgotPasswordCommandHandler(
    IAppUserRepository appUserRepository,
    ITenantRepository tenantRepository,
    IPasswordResetMailService passwordResetMailService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    // hesap olsun olmasın aynı cevap; hangi mail adreslerinin kayıtlı olduğu anlaşılmasın
    private const string Message = "Bu mail adresiyle bir hesap varsa şifre sıfırlama linki gönderildi. Lütfen gelen kutunuzu kontrol edin";

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await appUserRepository.GetByEmailIgnoringTenantAsync(new Email(request.Email), cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Message;
        }

        // giriş yapamayacak hesaba link gönderilmez
        if (!await tenantRepository.AnyAsync(t => t.Id == user.TenantId && t.IsActive, cancellationToken))
        {
            return Message;
        }

        var code = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        user.CreateForgotPasswordCode(ForgotPasswordCode.FromPlainText(code), timeProvider.GetUtcNow());
        appUserRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // linkin açık hali sadece burada var, bu yüzden e-posta bu istekte gönderilir
        await passwordResetMailService.SendAsync(user, code, cancellationToken);

        return Message;
    }
}
