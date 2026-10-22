using TS.MediatR;
using TS.Result;

namespace Zamanla.Application.Permissions;

// personel hesabındaki yetki kutuları için
public sealed record PermissionGetAllQuery : IRequest<Result<List<PermissionDefinition>>>;

internal sealed class PermissionGetAllQueryHandler : IRequestHandler<PermissionGetAllQuery, Result<List<PermissionDefinition>>>
{
    public Task<Result<List<PermissionDefinition>>> Handle(PermissionGetAllQuery request, CancellationToken cancellationToken)
    {
        Result<List<PermissionDefinition>> result = PermissionCatalog.All.ToList();
        return Task.FromResult(result);
    }
}
