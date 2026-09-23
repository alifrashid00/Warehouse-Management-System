using Microsoft.AspNetCore.Diagnostics;
using StockFlow.Api.Contracts;

namespace StockFlow.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception. CorrelationId: {CorrelationId}, Path: {Path}",
            correlationId,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = ApiResponse<object>.Fail(
            "INTERNAL_SERVER_ERROR",
            $"An unexpected error occurred. Reference: {correlationId}");

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
