using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Zamanla.Application.Services;

namespace Zamanla.Infrastructure.Services;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid? GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
