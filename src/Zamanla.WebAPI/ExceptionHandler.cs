using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using TS.Result;

namespace Zamanla.WebAPI;

internal sealed class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Result<string> result;

        if (exception is ValidationException validationException)
        {
            var messages = validationException.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            result = Result<string>.Failure(StatusCodes.Status422UnprocessableEntity, messages);
        }
        else
        {
            logger.LogError(exception, "Beklenmeyen hata: {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            result = Result<string>.Failure(
                StatusCodes.Status500InternalServerError,
                "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
        }

        httpContext.Response.StatusCode = result.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);

        return true;
    }
}
