namespace Orders.Infrastructure.Persistence;

public sealed class OrderEntity
{
    public Guid Id { get; set; }

    public string CustomerId { get; set; } = null!;

    public decimal Total { get; set; }

    public List<OrderItemEntity> Items { get; set; } = [];
}