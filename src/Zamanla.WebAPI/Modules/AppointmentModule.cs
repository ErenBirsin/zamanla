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
    }
}
