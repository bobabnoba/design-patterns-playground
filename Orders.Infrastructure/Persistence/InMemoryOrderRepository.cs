using System.Collections.Concurrent;
using Orders.Application.Orders.Abstractions;
using Orders.Domain.Orders;

namespace Orders.Infrastructure.Persistence;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _store = new();

    public Task Add(Order order, CancellationToken ct)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<Order?> Get(Guid id, CancellationToken ct)
    {
        _store.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }
}