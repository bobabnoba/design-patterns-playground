using Poc.MiddlewarePipeline.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionToProblemDetailsMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

app.MapGet("/ok", (HttpContext ctx) =>
{
    var cid = ctx.Items.TryGetValue("X-Correlation-Id", out var v) ? v?.ToString() : null;
    return Results.Ok(new { message = "ok", correlationId = cid });
});

app.MapGet("/boom", () =>
{
    throw new InvalidOperationException("Boom!");
});

app.Run();