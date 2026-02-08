using Orders.Application.Mediator;
using Orders.Application.Orders.Abstractions;
using Orders.Application.Orders.Dtos;

namespace Orders.Application.Orders.GetOrder;

public sealed class GetOrderHandler(IOrderRepository repo)
    : IHandler<GetOrderQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await repo.Get(request.Id, ct);
        if (order is null) return null;

        var items = order.Items
            .Select(i => new OrderItemDto(i.Sku, i.Quantity, i.UnitPrice))
            .ToList();

        return new OrderDto(order.Id, order.CustomerId, order.Total, items);
    }
}