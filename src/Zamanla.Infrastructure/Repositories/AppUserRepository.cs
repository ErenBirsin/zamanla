using GenericRepository;
using Zamanla.Domain.Users;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class AppUserRepository : Repository<AppUser, ApplicationDbContext>, IAppUserRepository
{
    public AppUserRepository(ApplicationDbContext context) : base(context)
    {
    }
}
