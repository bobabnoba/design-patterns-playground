namespace Poc.CqrsMediator.Api.Contracts;

public sealed record CreateOrderRequest(string CustomerId, IReadOnlyList<CreateOrderItemRequest> Items);