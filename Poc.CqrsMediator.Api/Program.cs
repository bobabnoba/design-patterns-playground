using Microsoft.AspNetCore.Mvc;
using Orders.Application.Behaviors;
using Orders.Application.Mediator;
using Orders.Application.Orders.Abstractions;
using Orders.Application.Orders.CreateOrder;
using Orders.Application.Orders.GetOrder;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;
using Orders.Infrastructure.Persistence;
using Poc.CqrsMediator.Api.Contracts;
using Orders.Application.Outbox;
using Poc.CqrsMediator.Api.Outbox;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMediator, Mediator>();

builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("OrdersDb")
    );
});

builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IOutbox, EfOutbox>();

builder.Services.AddTransient<IHandler<CreateOrderCommand, Guid>, CreateOrderHandler>();
builder.Services.AddTransient<IHandler<GetOrderQuery, Orders.Application.Orders.Dtos.OrderDto?>, GetOrderHandler>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddTransient<IValidator<CreateOrderCommand>, CreateOrderValidator>();

builder.Services.AddHostedService<OutboxPublisherWorker>();

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
