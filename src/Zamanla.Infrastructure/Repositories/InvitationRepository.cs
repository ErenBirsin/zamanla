using GenericRepository;
using Zamanla.Domain.Invitations;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class InvitationRepository : Repository<Invitation, ApplicationDbContext>, IInvitationRepository
{
    public InvitationRepository(ApplicationDbContext context) : base(context)
    {
    }
}
