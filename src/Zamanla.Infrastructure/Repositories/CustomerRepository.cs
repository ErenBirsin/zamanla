using Zamanla.Domain.Customers;
using Zamanla.Infrastructure.Abstractions;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;
internal sealed class CustomerRepository : AuditableRepository<Customer, ApplicationDbContext>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }
}
