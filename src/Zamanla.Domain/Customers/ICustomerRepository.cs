using Zamanla.Domain.Abstractions;

namespace Zamanla.Domain.Customers;
public interface ICustomerRepository : IAuditableRepository<Customer>
{
}
