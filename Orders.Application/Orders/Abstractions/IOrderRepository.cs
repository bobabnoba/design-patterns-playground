using Orders.Domain.Orders;

namespace Orders.Application.Orders.Abstractions;

public interface IOrderRepository
{
    Task Add(Order order, CancellationToken ct);
    Task<Order?> Get(Guid id, CancellationToken ct);
}