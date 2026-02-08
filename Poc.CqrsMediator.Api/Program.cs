using Microsoft.AspNetCore.Mvc;
using Orders.Application.Behaviors;
using Orders.Application.Mediator;
using Orders.Application.Orders.Abstractions;
using Orders.Application.Orders.CreateOrder;
using Orders.Application.Orders.GetOrder;
using Orders.Infrastructure.Persistence;
using Poc.CqrsMediator.Api.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DI: Core CQRS plumbing ---
builder.Services.AddSingleton<IMediator, Mediator>();

// Repo (in-memory for PoC #2)
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// Handlers (explicit for this PoC)
builder.Services.AddTransient<IHandler<CreateOrderCommand, Guid>, CreateOrderHandler>();
builder.Services.AddTransient<IHandler<GetOrderQuery, Orders.Application.Orders.Dtos.OrderDto?>, GetOrderHandler>();

// Behaviors (Decorator chain; order matters: first registered = outermost)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Validators
builder.Services.AddTransient<IValidator<CreateOrderCommand>, CreateOrderValidator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// TODO: exception handler
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (ArgumentException ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        ctx.Response.ContentType = "application/json";

        var problem = new ProblemDetails
        {
            Title = "Bad Request",
            Status = 400,
            Detail = ex.Message,
            Type = "about:blank"
        };

        await ctx.Response.WriteAsJsonAsync(problem);
    }
});

// --- Endpoints ---
app.MapPost("/orders", async (CreateOrderRequest req, IMediator mediator, CancellationToken ct) =>
{
    var cmd = new CreateOrderCommand(
        req.CustomerId,
        req.Items.Select(i => new CreateOrderItem(i.Sku, i.Quantity, i.UnitPrice)).ToList()
    );

    var id = await mediator.Send(cmd, ct);
    return Results.Created($"/orders/{id}", new { id });
});

app.MapGet("/orders/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var dto = await mediator.Send(new GetOrderQuery(id), ct);
    return dto is null ? Results.NotFound() : Results.Ok(dto);
});

app.Run();
