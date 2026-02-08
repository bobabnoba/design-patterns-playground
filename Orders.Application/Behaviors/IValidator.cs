namespace Orders.Application.Behaviors;

public interface IValidator<in TRequest>
{
    void Validate(TRequest request);
}