using Orders.Application.Abstractions;
using Orders.Application.Mediator;

namespace Orders.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork uow)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        await uow.BeginAsync(ct);

        try
        {
            var response = await next();
            await uow.CommitAsync(ct);
            return response;
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }
    }
}