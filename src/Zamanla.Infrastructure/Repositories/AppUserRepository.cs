using GenericRepository;
using Zamanla.Domain.Users;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;
internal sealed class AppUserRepository(ApplicationDbContext context)
    : Repository<AppUser, ApplicationDbContext>(context), IAppUserRepository;
