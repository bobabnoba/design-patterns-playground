using Microsoft.Extensions.DependencyInjection;

namespace Orders.Application.Mediator;

public sealed class Mediator(IServiceProvider sp) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        // Resolve IHandler<TRequest, TResponse>
        var handlerType = typeof(IHandler<,>).MakeGenericType(requestType, responseType);
        var handlerObj = sp.GetRequiredService(handlerType);

        // Resolve behaviors IEnumerable<IPipelineBehavior<TRequest,TResponse>>
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = sp.GetServices(behaviorType).Cast<dynamic>().ToList();

        Task<TResponse> HandlerCall()
            => ((dynamic)handlerObj).Handle((dynamic)request, ct);

        RequestHandlerDelegate<TResponse> pipeline = HandlerCall;

        // Compose decorators: first registered is outermost
        for (var i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;
            pipeline = () => (Task<TResponse>)behavior.Handle((dynamic)request, next, ct);
        }

        return pipeline();
    }
}