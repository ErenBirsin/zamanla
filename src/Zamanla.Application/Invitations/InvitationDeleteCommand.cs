using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Domain.Invitations;

namespace Zamanla.Application.Invitations;

// yanlış adrese gönderilen davet geri alınır; link artık açılmaz
[Permission]
public sealed record InvitationDeleteCommand(Guid StaffId) : IRequest<Result<string>>;

internal sealed class InvitationDeleteCommandHandler(
    IInvitationRepository invitationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<InvitationDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InvitationDeleteCommand request, CancellationToken cancellationToken)
    {
        var pending = await invitationRepository
            .WhereWithTracking(i => i.StaffId == request.StaffId && i.AcceptedAt == null)
            .ToListAsync(cancellationToken);
        if (pending.Count == 0)
        {
            return Result<string>.Failure(404, "Bekleyen davet bulunamadı");
        }

        invitationRepository.DeleteRange(pending);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Davet geri alındı";
    }
}
