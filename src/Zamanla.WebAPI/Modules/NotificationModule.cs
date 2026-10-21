using TS.MediatR;
using TS.Result;
using Zamanla.Application.Notifications;

namespace Zamanla.WebAPI.Modules;

public static class NotificationModule
{
    public static void MapNotification(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/notifications")
            .RequireAuthorization()
            .WithTags("Notifications");

        app.MapPut("{id}/read",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new NotificationReadCommand(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapPut("read-all",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new NotificationReadAllCommand(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();
    }
}
