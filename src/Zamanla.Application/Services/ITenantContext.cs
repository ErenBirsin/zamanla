namespace Zamanla.Application.Services;

public interface ITenantContext
{
    Guid? GetTenantId();

    // herkese açık uçlarda salon adresteki slug'dan çözülür
    void SetTenantId(Guid tenantId);
}
