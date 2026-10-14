using Zamanla.Domain.Staffs;
using Zamanla.Infrastructure.Abstractions;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class StaffRepository : AuditableRepository<Staff, ApplicationDbContext>, IStaffRepository
{
    public StaffRepository(ApplicationDbContext context) : base(context)
    {
    }
}
