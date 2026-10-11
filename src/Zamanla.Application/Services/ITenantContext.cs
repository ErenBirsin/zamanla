namespace Zamanla.Application.Services;

public interface ITenantContext
{
    Guid? GetTenantId();
}
