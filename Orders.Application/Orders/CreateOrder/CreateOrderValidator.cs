using Orders.Application.Behaviors;

namespace Orders.Application.Orders.CreateOrder;

public sealed class CreateOrderValidator : IValidator<CreateOrderCommand>
{
    public void Validate(CreateOrderCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerId))
            throw new ArgumentException("CustomerId is required.");

        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException("Items must not be empty.");

        if (request.Items.Any(i => string.IsNullOrWhiteSpace(i.Sku)))
            throw new ArgumentException("Item Sku is required.");

        if (request.Items.Any(i => i.Quantity <= 0))
            throw new ArgumentException("Item Quantity must be > 0.");

        if (request.Items.Any(i => i.UnitPrice < 0))
            throw new ArgumentException("Item UnitPrice must be >= 0.");
    }
}