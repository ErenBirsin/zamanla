using System.Buffers.Text;
using System.Security.Cryptography;
using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;
using Zamanla.Application.Services;
using Zamanla.Domain.Invitations;
using Zamanla.Domain.Invitations.ValueObjects;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Users;

namespace Zamanla.Application.Invitations;

// yetki adı yok: personel davet etmek sadece salon sahibine açık
[Permission]
public sealed record InvitationCreateCommand(
    Guid StaffId,
    string Email,
    List<string> Permissions) : IRequest<Result<InvitationCreatedDto>>;

public sealed record InvitationCreatedDto(string Link, bool EmailSent, DateTimeOffset ExpiresAt);

public sealed class InvitationCreateCommandValidator : AbstractValidator<InvitationCreateCommand>
{
    public InvitationCreateCommandValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Geçerli bir mail adresi girin")
            .EmailAddress().WithMessage("Geçerli bir mail adresi girin")
            .MaximumLength(254).WithMessage("Mail adresi en fazla 254 karakter olabilir");
        RuleForEach(p => p.Permissions)
            .Must(PermissionCatalog.Exists).WithMessage("Geçersiz bir yetki seçildi");
    }
}

internal sealed class InvitationCreateCommandHandler(
    IStaffRepository staffRepository,
    IAppUserRepository appUserRepository,
    IInvitationRepository invitationRepository,
    IInvitationMailService invitationMailService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<InvitationCreateCommand, Result<InvitationCreatedDto>>
{
    public async Task<Result<InvitationCreatedDto>> Handle(InvitationCreateCommand request, CancellationToken cancellationToken)
    {
        #region Kontroller
        var staff = await staffRepository.FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken, isTrackingActive: false);
        if (staff is null)
        {
            return Result<InvitationCreatedDto>.Failure(404, "Personel bulunamadı");
        }

        if (!staff.IsActive)
        {
            return Result<InvitationCreatedDto>.Failure(400, "Pasif personele davet gönderilemez");
        }

        if (await appUserRepository.AnyAsync(u => u.StaffId == request.StaffId, cancellationToken))
        {
            return Result<InvitationCreatedDto>.Failure(409, "Bu personelin zaten bir hesabı var");
        }

        Email email = new(request.Email);
        if (await appUserRepository.GetByEmailIgnoringTenantAsync(email, cancellationToken) is not null)
        {
            return Result<InvitationCreatedDto>.Failure(409, "Bu mail adresiyle açılmış bir hesap var");
        }
        #endregion

        // yeni davet eskisinin yerine geçer; eski link artık açılmaz
        var pending = await invitationRepository
            .WhereWithTracking(i => i.StaffId == request.StaffId && i.AcceptedAt == null)
            .ToListAsync(cancellationToken);
        invitationRepository.DeleteRange(pending);

        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        Invitation invitation = new(
            request.StaffId,
            email,
            InvitationToken.FromPlainText(token),
            request.Permissions.Distinct().Select(p => new Permission(p)),
            timeProvider.GetUtcNow());
        invitationRepository.Add(invitation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // linkin açık hali sadece burada var, bu yüzden e-posta bu istekte gönderilir
        var link = await invitationMailService.SendAsync(invitation, token, cancellationToken);

        return new InvitationCreatedDto(link.Url, link.EmailSent, invitation.ExpiresAt);
    }
}
