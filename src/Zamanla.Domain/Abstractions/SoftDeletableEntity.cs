namespace Zamanla.Domain.Abstractions;
public abstract class SoftDeletableEntity : Entity
{
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public void Delete()
    {
        IsDeleted = true;
    }
}
