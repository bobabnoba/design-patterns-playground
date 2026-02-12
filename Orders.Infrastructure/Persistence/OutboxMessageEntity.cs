namespace Orders.Infrastructure.Persistence;

public sealed class OutboxMessageEntity
{
    public Guid Id { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public string Type { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime? ProcessedAtUtc { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }
    
    public DateTime? LockedUntilUtc { get; set; }
    
    public string? LockedBy { get; set; }
    
    public DateTime? NextAttemptAtUtc { get; set; }
}
