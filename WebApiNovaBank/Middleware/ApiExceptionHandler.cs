using System.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApiNovaBank.Middleware;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DBConcurrencyException)
        {
            return false;
        }

        logger.LogInformation(exception, "A concurrent account update was rejected.");
        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Concurrent update",
            Detail = "The account changed during this request. Reload it and retry.",
            Instance = httpContext.Request.Path
        }, cancellationToken);
        return true;
    }
}
