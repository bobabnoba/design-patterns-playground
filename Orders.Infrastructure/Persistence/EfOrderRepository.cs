using Microsoft.EntityFrameworkCore;
using Orders.Application.Orders.Abstractions;
using Orders.Domain.Orders;

namespace Orders.Infrastructure.Persistence;

public sealed class EfOrderRepository(OrdersDbContext db) : IOrderRepository
{
    public async Task Add(Order order, CancellationToken ct)
    {
        var entity = new OrderEntity
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Total = order.Total,
            Items = order.Items.Select(i => new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        db.Orders.Add(entity);

        // TODO: move SaveChanges into TransactionBehavior
        await db.SaveChangesAsync(ct);
    }

    public async Task<Order?> Get(Guid id, CancellationToken ct)
    {
        var entity = await db.Orders
            .Include(o => o.Items)
            .AsNoTracking() // queries should not track
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (entity is null)
            return null;

        var items = entity.Items
            .Select(i => new OrderItem(i.Sku, i.Quantity, i.UnitPrice))
            .ToList();

        return new Order(entity.Id, entity.CustomerId, items);
    }
}