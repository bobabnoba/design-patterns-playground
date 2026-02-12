using System.Text.Json;
using Orders.Application.Mediator;
using Orders.Application.Orders.Abstractions;
using Orders.Application.Outbox;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.CreateOrder;

public sealed class CreateOrderHandler(IOrderRepository repo, IOutbox outbox)
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
        
        var evt = new 
        {
            orderId = id,
            customerId = request.CustomerId,
            total = order.Total
        };

        outbox.Enqueue(
            type: "OrderPlaced",
            payloadJson: JsonSerializer.Serialize(evt)
        );

        return id;
    }
}