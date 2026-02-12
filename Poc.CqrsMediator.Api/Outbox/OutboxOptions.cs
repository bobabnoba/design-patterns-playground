namespace Poc.CqrsMediator.Api.Outbox;

public sealed class OutboxOptions
{
    public bool Enabled { get; init; } = true;
    public int PollIntervalSeconds { get; init; } = 2;
    public int BatchSize { get; init; } = 20;
    public int LockDurationSeconds { get; init; } = 30;
    public int MaxAttempts { get; init; } = 10;

    public int BaseRetryDelaySeconds { get; init; } = 2;
    public int MaxRetryDelaySeconds { get; init; } = 60;

    public string[] FailFirstPublishForTypes { get; init; } = Array.Empty<string>();
}