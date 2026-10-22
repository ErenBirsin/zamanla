using Zamanla.Application.Auth;
using Zamanla.Application.Services;
using TS.MediatR;
using TS.Result;

namespace Zamanla.WebAPI.Modules;

public static class AuthModule
{
    public static void MapAuth(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/api/auth").WithTags("Auth");

        app.MapPost("/register",
            async (RegisterBusinessCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<AuthTokens>>()
            .RequireRateLimiting("login-fixed");

        app.MapPost("/login",
            async (LoginCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<AuthTokens>>()
            .RequireRateLimiting("login-fixed");

        app.MapPost("/refresh",
            async (RefreshTokenCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<AuthTokens>>();

        app.MapPost("/forgot-password",
            async (ForgotPasswordCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>()
            .RequireRateLimiting("login-fixed");

        app.MapGet("/check-forgot-password-code/{forgotPasswordCode}",
            async (string forgotPasswordCode, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new CheckForgotPasswordCodeCommand(forgotPasswordCode), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<bool>>()
            .RequireRateLimiting("login-fixed");

        app.MapPost("/reset-password",
            async (ResetPasswordCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<string>>()
            .RequireRateLimiting("login-fixed");

        app.MapGet("/me",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new CurrentUserGetQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.Json(res, statusCode: res.StatusCode);
            })
            .Produces<Result<CurrentUserDto>>()
            .RequireAuthorization();
    }
}
