namespace Orders.Application.Orders.Dtos;

public sealed record OrderDto(Guid Id, string CustomerId, decimal Total, IReadOnlyList<OrderItemDto> Items);