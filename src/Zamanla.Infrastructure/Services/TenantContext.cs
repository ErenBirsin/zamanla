using Microsoft.AspNetCore.Http;
using Zamanla.Application.Services;

namespace Zamanla.Infrastructure.Services;

internal sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private Guid? _tenantId;

    public Guid? GetTenantId()
    {
        // adresten çözülen salon oturumdaki salondan önce gelir; panele giriş yapmış biri başka salonun sayfasını açabilir
        if (_tenantId is not null)
        {
            return _tenantId;
        }

        var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimNames.TenantId)?.Value;

        return Guid.TryParse(value, out var tenantId) ? tenantId : null;
    }

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}
