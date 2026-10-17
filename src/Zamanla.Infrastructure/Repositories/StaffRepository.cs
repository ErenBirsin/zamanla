using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Staffs;
using Zamanla.Infrastructure.Abstractions;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class StaffRepository : AuditableRepository<Staff, ApplicationDbContext>, IStaffRepository
{
    private readonly ApplicationDbContext _context;

    public StaffRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public IQueryable<Staff> GetAllIncludingDeleted()
    {
        return _context.Set<Staff>()
            .AsNoTracking()
            .IgnoreQueryFilters([QueryFilterExtensions.SoftDeleteFilter]);
    }
}
