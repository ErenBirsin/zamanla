using GenericRepository;
using Zamanla.Domain.Shared;

namespace Zamanla.Domain.Users;

public interface IAppUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByEmailIgnoringTenantAsync(Email email, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByIdIgnoringTenantAsync(Guid id, CancellationToken cancellationToken = default);
}
