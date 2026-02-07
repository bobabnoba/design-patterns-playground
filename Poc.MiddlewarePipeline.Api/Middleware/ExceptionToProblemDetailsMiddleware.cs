using System.Text.Json;

namespace Poc.MiddlewarePipeline.Api.Middleware;

public sealed class ExceptionToProblemDetailsMiddleware(
    RequestDelegate next,
    ILogger<ExceptionToProblemDetailsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items.TryGetValue("X-Correlation-Id", out var cid)
                ? cid?.ToString()
                : null;

            logger.LogError(ex, "Unhandled exception (cid={CorrelationId})", correlationId);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                type = "about:blank",
                title = "Internal Server Error",
                status = 500,
                detail = "An unexpected error occurred.",
                correlationId = correlationId
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
