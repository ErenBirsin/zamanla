using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.SalonServices;
using Zamanla.Infrastructure.Abstractions;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class SalonServiceRepository : AuditableRepository<SalonService, ApplicationDbContext>, ISalonServiceRepository
{
    private readonly ApplicationDbContext _context;

    public SalonServiceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public IQueryable<SalonService> GetAllIncludingDeleted()
    {
        return _context.Set<SalonService>()
            .AsNoTracking()
            .IgnoreQueryFilters([QueryFilterExtensions.SoftDeleteFilter]);
    }
}
