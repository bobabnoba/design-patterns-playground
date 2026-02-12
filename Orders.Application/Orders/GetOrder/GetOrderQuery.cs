using Orders.Application.Mediator;
using Orders.Application.Orders.Dtos;

namespace Orders.Application.Orders.GetOrder;

public sealed record GetOrderQuery(Guid Id) : IQuery<OrderDto?>;