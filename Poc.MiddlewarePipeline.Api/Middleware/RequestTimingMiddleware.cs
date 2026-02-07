using System.Diagnostics;

namespace Poc.MiddlewarePipeline.Api.Middleware;

public sealed class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            // TTFB
            context.Response.Headers["X-Elapsed-Ms"] = sw.ElapsedMilliseconds.ToString();
            return Task.CompletedTask;
        });

        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();

            var correlationId = context.Items.TryGetValue("X-Correlation-Id", out var cid)
                ? cid?.ToString()
                : null;

            logger.LogInformation(
                "HTTP {Method} {Path} -> {StatusCode} in {ElapsedMs}ms (cid={CorrelationId})",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId
            );
        }
    }
}