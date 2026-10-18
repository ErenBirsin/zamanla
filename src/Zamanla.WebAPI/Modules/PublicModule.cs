using TS.MediatR;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Public;

namespace Zamanla.WebAPI.Modules;

public static class PublicModule
{
    public static void MapPublic(this IEndpointRouteBuilder builder)
    {
        // giriş gerektirmez; salon adresteki slug'dan bulunur
        var app = builder
            .MapGroup("/api/public/{slug}")
            .AddEndpointFilter<PublicTenantFilter>()
            .RequireRateLimiting("public-fixed")
            .WithTags("Public");

        app.MapGet(string.Empty,
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new PublicSalonGetQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<PublicSalonDto>>();

        app.MapGet("availability",
            async (DateOnly date, Guid[] salonServiceIds, Guid? staffId, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new PublicAvailabilityGetQuery(date, [.. salonServiceIds], staffId), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<List<AvailabilitySlotDto>>>();
    }
}
