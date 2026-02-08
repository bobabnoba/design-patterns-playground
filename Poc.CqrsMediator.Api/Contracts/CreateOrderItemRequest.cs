namespace Poc.CqrsMediator.Api.Contracts;

public sealed record CreateOrderItemRequest(string Sku, int Quantity, decimal UnitPrice);