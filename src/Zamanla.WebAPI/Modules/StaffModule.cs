using TS.MediatR;
using TS.Result;
using Zamanla.Application.Staffs;

namespace Zamanla.WebAPI.Modules;

public static class StaffModule
{
    public static void MapStaff(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/staff")
            .RequireAuthorization()
            .WithTags("Staff");

        app.MapPost(string.Empty,
            async (StaffCreateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapPut(string.Empty,
            async (StaffUpdateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapDelete("{id}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new StaffDeleteCommand(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapGet("{id}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new StaffGetQuery(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<StaffDto>>();

        // hesap uçları sadece salon sahibine açık
        app.MapGet("accounts",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new StaffAccountGetAllQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<List<StaffAccountDto>>>();

        app.MapPut("accounts/permissions",
            async (StaffAccountPermissionsUpdateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();

        app.MapPut("accounts/status",
            async (StaffAccountStatusUpdateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>();
    }
}
