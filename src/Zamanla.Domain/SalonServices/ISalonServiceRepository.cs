using Zamanla.Domain.Abstractions;

namespace Zamanla.Domain.SalonServices;

public interface ISalonServiceRepository : IAuditableRepository<SalonService>
{
    // geçmiş randevularda silinmiş hizmetin adı da görünsün
    IQueryable<SalonService> GetAllIncludingDeleted();
}
