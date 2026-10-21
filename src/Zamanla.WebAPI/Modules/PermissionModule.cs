using TS.MediatR;
using TS.Result;
using Zamanla.Application.Permissions;

namespace Zamanla.WebAPI.Modules;

public static class PermissionModule
{
    public static void MapPermission(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/permissions")
            .RequireAuthorization()
            .WithTags("Permissions");

        app.MapGet(string.Empty,
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new PermissionGetAllQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<List<PermissionDefinition>>>();
    }
}
