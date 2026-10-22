using GenericRepository;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.Domain.Users;

public interface IAppUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByEmailIgnoringTenantAsync(Email email, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByIdIgnoringTenantAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByForgotPasswordCodeIgnoringTenantAsync(ForgotPasswordCode code, CancellationToken cancellationToken = default);
}
