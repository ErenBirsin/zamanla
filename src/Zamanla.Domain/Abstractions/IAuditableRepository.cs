using GenericRepository;
using Zamanla.Domain.Users;

namespace Zamanla.Domain.Abstractions;

public interface IAuditableRepository<TEntity> : IRepository<TEntity>
    where TEntity : Entity
{
    IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAudit();
}
public sealed class EntityWithAuditDto<TEntity>
    where TEntity : Entity
{
    public TEntity Entity { get; set; } = default!;
    // oturumsuz yapılan kayıtlarda (ör. işletme kaydı) kullanıcı yok
    public AppUser? CreatedUser { get; set; }
    public AppUser? UpdatedUser { get; set; }
}
