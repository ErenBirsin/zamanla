using TS.MediatR;
using TS.Result;
using Zamanla.Application.Appointments;

namespace Zamanla.WebAPI.Modules;

public static class AppointmentModule
{
    public static void MapAppointment(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/appointments")
            .RequireAuthorization()
            .WithTags("Appointments");

        app.MapPost(string.Empty,
            async (AppointmentCreateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapGet("{id}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new AppointmentGetQuery(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<AppointmentDto>>();


        app.MapPut("{id}/complete",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new AppointmentCompleteCommand(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapPut("{id}/no-show",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new AppointmentNoShowCommand(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapPut("{id}/cancel",
            async (Guid id, AppointmentCancelCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request with { Id = id }, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();
    }
}
