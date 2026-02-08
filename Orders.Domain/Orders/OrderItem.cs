namespace Orders.Domain.Orders;

public sealed record OrderItem(string Sku, int Quantity, decimal UnitPrice);