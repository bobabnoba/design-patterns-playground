namespace Orders.Infrastructure.Persistence;

public sealed class OrderItemEntity
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string Sku { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public OrderEntity Order { get; set; } = null!;
}