namespace Orders.Application.Orders.Dtos;

public sealed record OrderItemDto(string Sku, int Quantity, decimal UnitPrice);