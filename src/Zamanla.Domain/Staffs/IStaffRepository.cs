using Zamanla.Domain.Abstractions;

namespace Zamanla.Domain.Staffs;

public interface IStaffRepository : IAuditableRepository<Staff>
{
    // geçmiş randevularda silinmiş personelin adı da görünsün
    IQueryable<Staff> GetAllIncludingDeleted();
}
