using TS.MediatR;
using TS.Result;
using Zamanla.Application.Invitations;
using Zamanla.Application.Services;
using Zamanla.Domain.Invitations;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Public;

// personelin davet linkinden açtığı şifre belirleme sayfası
public sealed record PublicInvitationGetQuery(string Token) : IRequest<Result<PublicInvitationDto>>;

public sealed record PublicInvitationDto(
    string SalonName,
    string StaffFullName,
    string Email,
    DateTimeOffset ExpiresAt);

internal sealed class PublicInvitationGetQueryHandler(
    IInvitationRepository invitationRepository,
    IStaffRepository staffRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : IRequestHandler<PublicInvitationGetQuery, Result<PublicInvitationDto>>
{
    public async Task<Result<PublicInvitationDto>> Handle(PublicInvitationGetQuery request, CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByTokenAsync(request.Token, cancellationToken);
        var check = InvitationRules.Check(invitation, timeProvider.GetUtcNow());
        if (check is not null)
        {
            return Result<PublicInvitationDto>.Failure(check.Value.StatusCode, check.Value.Message);
        }

        var staff = await staffRepository.FirstOrDefaultAsync(s => s.Id == invitation!.StaffId, cancellationToken, isTrackingActive: false);
        var tenant = await tenantRepository.FirstAsync(t => t.Id == tenantContext.GetTenantId(), cancellationToken, isTrackingActive: false);

        return new PublicInvitationDto(
            tenant.Name.Value,
            staff?.FullName ?? string.Empty,
            invitation!.Email.Value,
            invitation.ExpiresAt);
    }
}
