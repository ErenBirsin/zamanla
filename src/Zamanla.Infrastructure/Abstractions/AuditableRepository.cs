using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Users;

namespace Zamanla.Infrastructure.Abstractions;

internal class AuditableRepository<TEntity, TContext> : Repository<TEntity, TContext>, IAuditableRepository<TEntity>
    where TEntity : Entity
    where TContext : DbContext
{
    private readonly TContext _context;

    public AuditableRepository(TContext context) : base(context)
    {
        _context = context;
    }
    public IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAudit()
    {
        var entities = _context.Set<TEntity>().AsQueryable();
        var users = _context.Set<AppUser>().AsNoTracking().AsQueryable();

        // CreatedBy boş olabildiği için iki birleştirme de left join
        return entities
            .GroupJoin(users, e => e.CreatedBy, u => (Guid?)u.Id,
                (entity, createdUsers) => new { entity, createdUsers })
            .SelectMany(x => x.createdUsers.DefaultIfEmpty(),
                (x, createdUser) => new { x.entity, createdUser })
            .GroupJoin(users, x => x.entity.UpdatedBy, u => (Guid?)u.Id,
                (x, updatedUsers) => new { x.entity, x.createdUser, updatedUsers })
            .SelectMany(x => x.updatedUsers.DefaultIfEmpty(),
                (x, updatedUser) => new EntityWithAuditDto<TEntity>
                {
                    Entity = x.entity,
                    CreatedUser = x.createdUser,
                    UpdatedUser = updatedUser
                });
    }
}
