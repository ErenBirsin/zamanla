using TS.MediatR;
using TS.Result;
using Zamanla.Application.TimeOffs;

namespace Zamanla.WebAPI.Modules;

public static class TimeOffModule
{
    public static void MapTimeOff(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/time-offs")
            .RequireAuthorization()
            .WithTags("TimeOffs");

        app.MapPost(string.Empty,
            async (TimeOffCreateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapDelete("{id}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new TimeOffDeleteCommand(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();
    }
}
