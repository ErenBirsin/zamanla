using System.Reflection;
using TS.MediatR;
using Zamanla.Application.Permissions;
using Zamanla.Application.Services;
using Zamanla.Domain.Users;

namespace Zamanla.Application.Behaviors;

// yetki her istekte veritabanından okunur; sahip kaldırınca ya da hesap pasif olunca hemen geçerli olur
public sealed class PermissionBehavior<TRequest, TResponse>(
    IUserContext userContext,
    IAppUserRepository appUserRepository) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var attr = request.GetType().GetCustomAttribute<PermissionAttribute>(inherit: true);
        if (attr is null)
        {
            return await next();
        }

        var userId = userContext.GetUserId();
        AppUser? user = userId is null
            ? null
            : await appUserRepository.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken, isTrackingActive: false);
        if (user is null || !user.IsActive)
        {
            throw new AuthorizationException("Hesabınız pasif durumda");
        }

        // yetki adı yoksa işlem sadece salon sahibine açık
        if (string.IsNullOrEmpty(attr.Permission))
        {
            if (!user.IsOwner)
            {
                throw new AuthorizationException("Bu işlemi sadece salon sahibi yapabilir");
            }
        }
        else if (!user.HasPermission(attr.Permission))
        {
            throw new AuthorizationException($"Bu işlem için \"{PermissionCatalog.GetLabel(attr.Permission)}\" yetkisi gerekiyor");
        }

        return await next();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class PermissionAttribute : Attribute
{
    public string? Permission { get; }

    public PermissionAttribute()
    {
    }

    public PermissionAttribute(string permission)
    {
        Permission = permission;
    }
}

public sealed class AuthorizationException : Exception
{
    public AuthorizationException() : base("Bu işlem için yetkiniz yok")
    {
    }

    public AuthorizationException(string message) : base(message)
    {
    }
}
