using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Domain.Invitations;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Users;

namespace Zamanla.Application.Staffs;

public static class StaffAccountStatus
{
    public const string None = "Hesap yok";
    public const string Invited = "Davet gönderildi";
    public const string InvitationExpired = "Davetin süresi doldu";
    public const string Active = "Aktif";
    public const string Inactive = "Pasif";
}

// her personelin hesap durumu; hesap yoksa bekleyen davet gösterilir
[Permission]
public sealed record StaffAccountGetAllQuery : IRequest<Result<List<StaffAccountDto>>>;

public sealed record StaffAccountDto(
    Guid StaffId,
    string StaffFullName,
    bool StaffIsActive,
    string Status,
    Guid? UserId,
    string? Email,
    List<string> Permissions,
    DateTimeOffset? InvitationExpiresAt);

internal sealed class StaffAccountGetAllQueryHandler(
    IStaffRepository staffRepository,
    IAppUserRepository appUserRepository,
    IInvitationRepository invitationRepository,
    TimeProvider timeProvider) : IRequestHandler<StaffAccountGetAllQuery, Result<List<StaffAccountDto>>>
{
    public async Task<Result<List<StaffAccountDto>>> Handle(StaffAccountGetAllQuery request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var staffList = await staffRepository.GetAll()
            .OrderBy(s => s.FirstName.Value)
            .ThenBy(s => s.LastName.Value)
            .Select(s => new { s.Id, FullName = s.FirstName.Value + " " + s.LastName.Value, s.IsActive })
            .ToListAsync(cancellationToken);
        var users = await appUserRepository.Where(u => u.StaffId != null).ToListAsync(cancellationToken);
        var invitations = await invitationRepository.Where(i => i.AcceptedAt == null).ToListAsync(cancellationToken);

        var result = staffList.Select(staff =>
        {
            var user = users.FirstOrDefault(u => u.StaffId == staff.Id);
            if (user is not null)
            {
                return new StaffAccountDto(
                    staff.Id,
                    staff.FullName,
                    staff.IsActive,
                    user.IsActive ? StaffAccountStatus.Active : StaffAccountStatus.Inactive,
                    user.Id,
                    user.Email.Value,
                    user.Permissions.Select(p => p.Value).ToList(),
                    null);
            }

            var invitation = invitations.FirstOrDefault(i => i.StaffId == staff.Id);
            if (invitation is not null)
            {
                return new StaffAccountDto(
                    staff.Id,
                    staff.FullName,
                    staff.IsActive,
                    invitation.IsExpired(now) ? StaffAccountStatus.InvitationExpired : StaffAccountStatus.Invited,
                    null,
                    invitation.Email.Value,
                    invitation.Permissions.Select(p => p.Value).ToList(),
                    invitation.ExpiresAt);
            }

            return new StaffAccountDto(staff.Id, staff.FullName, staff.IsActive, StaffAccountStatus.None, null, null, [], null);
        }).ToList();

        return result;
    }
}
