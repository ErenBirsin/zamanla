using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Invitations;
using Zamanla.Application.Services;
using Zamanla.Domain.Invitations;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Users;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.Application.Public;

// personel şifresini belirler, hesabı açılır ve giriş yapmış olur
public sealed record PublicInvitationAcceptCommand(
    string Token,
    string Password) : IRequest<Result<AuthTokens>>;

public sealed class PublicInvitationAcceptCommandValidator : AbstractValidator<PublicInvitationAcceptCommand>
{
    public PublicInvitationAcceptCommandValidator()
    {
        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("Geçerli bir şifre girin")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalı")
            .Matches(@"\p{Lu}").WithMessage("Şifre en az bir büyük harf içermeli")
            .Matches(@"\p{Ll}").WithMessage("Şifre en az bir küçük harf içermeli")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermeli")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>]").WithMessage("Şifre en az bir özel karakter içermeli");
    }
}

internal sealed class PublicInvitationAcceptCommandHandler(
    IInvitationRepository invitationRepository,
    IStaffRepository staffRepository,
    IAppUserRepository appUserRepository,
    IUnitOfWork unitOfWork,
    IJwtProvider jwtProvider,
    TimeProvider timeProvider) : IRequestHandler<PublicInvitationAcceptCommand, Result<AuthTokens>>
{
    public async Task<Result<AuthTokens>> Handle(PublicInvitationAcceptCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        #region Kontroller
        var invitation = await invitationRepository.GetByTokenAsync(request.Token, cancellationToken);
        var check = InvitationRules.Check(invitation, now);
        if (check is not null)
        {
            return Result<AuthTokens>.Failure(check.Value.StatusCode, check.Value.Message);
        }

        // davetten sonra silinen ya da pasif yapılan personel hesap açamaz
        var staff = await staffRepository.FirstOrDefaultAsync(s => s.Id == invitation!.StaffId, cancellationToken, isTrackingActive: false);
        if (staff is null || !staff.IsActive)
        {
            return Result<AuthTokens>.Failure(400, "Personel kaydı aktif değil, salon sahibiyle görüşün");
        }

        if (await appUserRepository.AnyAsync(u => u.StaffId == staff.Id, cancellationToken))
        {
            return Result<AuthTokens>.Failure(409, "Bu personelin zaten bir hesabı var");
        }

        if (await appUserRepository.GetByEmailIgnoringTenantAsync(invitation!.Email, cancellationToken) is not null)
        {
            return Result<AuthTokens>.Failure(409, "Bu mail adresiyle açılmış bir hesap var, giriş yapın");
        }
        #endregion

        // değer nesneleri iki kayıt arasında paylaşılmaz; yenileri oluşturulur
        AppUser user = new(
            invitation.TenantId,
            new FirstName(staff.FirstName.Value),
            new LastName(staff.LastName.Value),
            new Email(invitation.Email.Value),
            new Password(request.Password),
            UserRole.Staff);
        user.SetStaffId(staff.Id);
        user.SetPermissions(invitation.Permissions.Select(p => new Permission(p.Value)));
        appUserRepository.Add(user);
        invitation.Accept(now);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // aynı davet iki kez aynı anda kabul edildiyse ikincisi personelin tekil hesap kuralına takılır
            return Result<AuthTokens>.Failure(409, "Bu davet zaten kullanıldı");
        }

        return await jwtProvider.CreateTokensAsync(user, cancellationToken);
    }
}
