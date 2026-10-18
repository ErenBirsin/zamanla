using Microsoft.EntityFrameworkCore;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Domain.Tenants;

namespace Zamanla.WebAPI;

// herkese açık uçlarda salon, giriş yapan kullanıcıdan değil adresteki slug'dan bulunur
internal sealed class PublicTenantFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var slug = (httpContext.Request.RouteValues["slug"] as string ?? string.Empty).ToLowerInvariant();

        // filtre bir kez oluşturulur; scoped servisler isteğin kendi kapsamından alınır
        var tenantRepository = httpContext.RequestServices.GetRequiredService<ITenantRepository>();
        var tenantId = await tenantRepository
            .Where(t => t.Slug.Value == slug && t.IsActive)
            .Select(t => t.Id)
            .FirstOrDefaultAsync(httpContext.RequestAborted);
        if (tenantId == Guid.Empty)
        {
            return Results.Json(Result<string>.Failure(404, "Salon bulunamadı"), statusCode: StatusCodes.Status404NotFound);
        }

        httpContext.RequestServices.GetRequiredService<ITenantContext>().SetTenantId(tenantId);

        return await next(context);
    }
}
