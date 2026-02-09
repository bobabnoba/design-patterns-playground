namespace Orders.Application.Mediator;

public interface ICommand<TResponse> : IRequest<TResponse>
{
}