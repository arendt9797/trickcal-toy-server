using Microsoft.AspNetCore.Diagnostics;
using TrickcalServer.Models;

namespace TrickcalServer.Infrastructure;

public class BannerExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, error) = exception switch
        {
            BannerNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            BannerExpiredException => (StatusCodes.Status409Conflict, exception.Message),
            InvalidBannerConfigException => (StatusCodes.Status400BadRequest, exception.Message),
            _ => (0, null)
        };

        if (statusCode == 0) return false;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new { error },
            cancellationToken);

        return true;
    }
}
