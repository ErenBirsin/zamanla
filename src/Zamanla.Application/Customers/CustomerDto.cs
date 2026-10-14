using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Customers;

namespace Zamanla.Application.Customers;

public sealed class CustomerDto : EntityDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!; // +905321234567 biçiminde
    public string? Email { get; set; }
    public string? Note { get; set; }
}

public static class CustomerExtensions
{
    public static IQueryable<CustomerDto> MapTo(this IQueryable<EntityWithAuditDto<Customer>> entities)
    {
        return entities
            .Where(s => !s.Entity.IsAnonymized)
            .Select(s => new CustomerDto
            {
                Id = s.Entity.Id,
                FirstName = s.Entity.FirstName.Value,
                LastName = s.Entity.LastName.Value,
                FullName = s.Entity.FirstName.Value + " " + s.Entity.LastName.Value,
                PhoneNumber = s.Entity.PhoneNumber!.Value,
                Email = s.Entity.Email != null ? s.Entity.Email.Value : null,
                Note = s.Entity.Note != null ? s.Entity.Note.Value : null,
                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy,
                CreatedFullName = s.CreatedUser != null
                    ? s.CreatedUser.FirstName.Value + " " + s.CreatedUser.LastName.Value
                    : null,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy,
                UpdatedFullName = s.UpdatedUser != null
                    ? s.UpdatedUser.FirstName.Value + " " + s.UpdatedUser.LastName.Value
                    : null,
            });
    }
}
