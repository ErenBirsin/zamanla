using GenericRepository;
using Zamanla.Domain.RefreshTokens;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class RefreshTokenRepository : Repository<RefreshToken, ApplicationDbContext>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}
