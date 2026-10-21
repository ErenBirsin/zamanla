using TS.MediatR;
using TS.Result;
using Zamanla.Application.Invitations;

namespace Zamanla.WebAPI.Modules;

public static class InvitationModule
{
    public static void MapInvitation(this IEndpointRouteBuilder builder)
    {
        var app = builder
            .MapGroup("/api/invitations")
            .RequireAuthorization()
            .WithTags("Invitations");

        app.MapPost(string.Empty,
            async (InvitationCreateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<InvitationCreatedDto>>();
    }
}
