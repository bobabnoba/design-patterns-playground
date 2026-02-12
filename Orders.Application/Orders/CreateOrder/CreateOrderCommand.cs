using Orders.Application.Mediator;

namespace Orders.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    string CustomerId,
    IReadOnlyList<CreateOrderItem> Items
) : ICommand<Guid>;

public sealed record CreateOrderItem(string Sku, int Quantity, decimal UnitPrice);