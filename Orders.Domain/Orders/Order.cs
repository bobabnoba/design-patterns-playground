namespace Orders.Domain.Orders;

public sealed class Order
{
    public Guid Id { get; }
    public string CustomerId { get; }
    public IReadOnlyList<OrderItem> Items { get; }
    public decimal Total { get; }

    public Order(Guid id, string customerId, IReadOnlyList<OrderItem> items)
    {
        Validate(customerId, items);

        Id = id;
        CustomerId = customerId;
        Items = items;

        Total = items.Sum(i => i.Quantity * i.UnitPrice);
    }
    
    private static void Validate(string customerId, IReadOnlyList<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        if (items is null || items.Count == 0)
            throw new ArgumentException("Order must have at least one item.", nameof(items));

        if (items.Any(i => string.IsNullOrWhiteSpace(i.Sku)))
            throw new ArgumentException("Item Sku is required.", nameof(items));

        if (items.Any(i => i.Quantity <= 0))
            throw new ArgumentException("Item Quantity must be > 0.", nameof(items));

        if (items.Any(i => i.UnitPrice < 0))
            throw new ArgumentException("Item UnitPrice must be >= 0.", nameof(items));
    }
}