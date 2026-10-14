namespace Zamanla.Domain.Abstractions;

public abstract class EntityDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? CreatedFullName { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? UpdatedFullName { get; set; }
}
