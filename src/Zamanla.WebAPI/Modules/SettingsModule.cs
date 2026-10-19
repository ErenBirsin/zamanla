using TS.MediatR;
using TS.Result;
using Zamanla.Application.Settings;

namespace Zamanla.WebAPI.Modules;

public static class SettingsModule
{
    public static void MapSettings(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/settings")
            .RequireAuthorization()
            .WithTags("Settings");

        app.MapGet(string.Empty,
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new SettingsGetQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<SettingsDto>>();

        app.MapPut(string.Empty,
            async (SettingsUpdateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();
    }
}
