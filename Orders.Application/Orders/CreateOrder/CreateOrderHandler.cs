using Orders.Application.Mediator;
using Orders.Application.Orders.Abstractions;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.CreateOrder;

public sealed class CreateOrderHandler(IOrderRepository repo)
    : IHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        var items = request.Items
            .Select(i => new OrderItem(i.Sku, i.Quantity, i.UnitPrice))
            .ToList();

        var order = new Order(id, request.CustomerId, items);

        await repo.Add(order, ct);

        return id;
    }
}