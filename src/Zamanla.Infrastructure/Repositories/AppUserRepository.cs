using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Users;
using Zamanla.Domain.Users.ValueObjects;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class AppUserRepository : Repository<AppUser, ApplicationDbContext>, IAppUserRepository
{
    private readonly ApplicationDbContext _context;

    public AppUserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<AppUser?> GetByEmailIgnoringTenantAsync(Email email, CancellationToken cancellationToken = default)
    {
        return _context.Set<AppUser>()
            .IgnoreQueryFilters([QueryFilterExtensions.TenantFilter])
            .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    public Task<AppUser?> GetByIdIgnoringTenantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Set<AppUser>()
            .IgnoreQueryFilters([QueryFilterExtensions.TenantFilter])
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<AppUser?> GetByForgotPasswordCodeIgnoringTenantAsync(ForgotPasswordCode code, CancellationToken cancellationToken = default)
    {
        var hash = code.Value;
        return _context.Set<AppUser>()
            .IgnoreQueryFilters([QueryFilterExtensions.TenantFilter])
            .FirstOrDefaultAsync(u => u.ForgotPasswordCode!.Value == hash, cancellationToken);
    }
}
