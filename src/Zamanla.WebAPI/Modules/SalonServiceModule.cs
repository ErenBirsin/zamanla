using TS.MediatR;
using TS.Result;
using Zamanla.Application.SalonServices;

namespace Zamanla.WebAPI.Modules;

public static class SalonServiceModule
{
    public static void MapSalonService(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/services")
            .RequireAuthorization()
            .WithTags("Services");

        app.MapPost(string.Empty,
            async (SalonServiceCreateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapPut(string.Empty,
            async (SalonServiceUpdateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapDelete("{id}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new SalonServiceDeleteCommand(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapGet("{id}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new SalonServiceGetQuery(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<SalonServiceDto>>();
    }
}
