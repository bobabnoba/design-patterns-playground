using Orders.Application.Outbox;

namespace Orders.Infrastructure.Persistence;

public sealed class EfOutbox(OrdersDbContext db) : IOutbox
{
    public void Enqueue(string type, string payloadJson)
    {
        db.OutboxMessages.Add(new OutboxMessageEntity
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            Type = type,
            Payload = payloadJson,
            ProcessedAtUtc = null,
            Attempts = 0,
            LastError = null
        });
    }
}