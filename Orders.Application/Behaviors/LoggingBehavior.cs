using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Orders.Application.Mediator;

namespace Orders.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var name = typeof(TRequest).Name;

        logger.LogInformation("Handling {RequestName}", name);

        try
        {
            var response = await next();
            logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed {RequestName} after {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}