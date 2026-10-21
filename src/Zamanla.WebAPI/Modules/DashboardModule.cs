using TS.MediatR;
using TS.Result;
using Zamanla.Application.Dashboard;

namespace Zamanla.WebAPI.Modules;

public static class DashboardModule
{
    public static void MapDashboard(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/dashboard")
            .RequireAuthorization()
            .WithTags("Dashboard");

        app.MapGet(string.Empty,
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new DashboardGetQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<DashboardDto>>();
    }
}
