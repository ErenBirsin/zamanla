using Microsoft.AspNetCore.Http;
using Zamanla.Application.Services;

namespace Zamanla.Infrastructure.Services;

internal sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid? GetTenantId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimNames.TenantId)?.Value;

        return Guid.TryParse(value, out var tenantId) ? tenantId : null;
    }
}
