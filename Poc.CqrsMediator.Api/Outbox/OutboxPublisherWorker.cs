using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Orders.Infrastructure.Persistence;

namespace Poc.CqrsMediator.Api.Outbox;

public sealed class OutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherWorker> logger,
    IOptions<OutboxOptions> optionsAccessor) : BackgroundService
{
    private readonly OutboxOptions _options = optionsAccessor.Value;

    private readonly string _workerId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Outbox worker disabled by config");
            return;
        }

        logger.LogInformation("Outbox worker started (workerId={WorkerId})", _workerId);

        var pollDelay = TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds));
        var lockDuration = TimeSpan.FromSeconds(Math.Max(5, _options.LockDurationSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var now = DateTime.UtcNow;

                var claimed = await ClaimBatch(db, now, lockDuration, stoppingToken);

                foreach (var msg in claimed)
                {
                    try
                    {
                        if (msg.Attempts == 0 && _options.FailFirstPublishForTypes.Contains(msg.Type))
                            throw new InvalidOperationException($"Simulated publish failure for type '{msg.Type}'");

                        logger.LogInformation(
                            "Publishing outbox message {Id} type={Type} payload={Payload}",
                            msg.Id, msg.Type, msg.Payload);

                        msg.ProcessedAtUtc = DateTime.UtcNow;
                        msg.LastError = null;

                        msg.LockedBy = null;
                        msg.LockedUntilUtc = null;
                        msg.NextAttemptAtUtc = null;
                    }
                    catch (Exception ex)
                    {
                        msg.Attempts += 1;
                        msg.LastError = ex.Message;

                        var delaySeconds = ComputeBackoffSeconds(msg.Attempts);
                        msg.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(delaySeconds);

                        msg.LockedBy = null;
                        msg.LockedUntilUtc = null;

                        logger.LogError(ex,
                            "Failed to publish outbox message {Id} (attempt {Attempt}/{MaxAttempts}), retry in {DelaySeconds}s",
                            msg.Id, msg.Attempts, _options.MaxAttempts, delaySeconds);
                    }
                }

                if (claimed.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox worker loop failed");
            }

            await Task.Delay(pollDelay, stoppingToken);
        }
    }

    private async Task<List<OutboxMessageEntity>> ClaimBatch(
        OrdersDbContext db,
        DateTime now,
        TimeSpan lockDuration,
        CancellationToken ct)
    {
        var batchSize = Math.Clamp(_options.BatchSize, 1, 200);
        var maxAttempts = Math.Max(1, _options.MaxAttempts);

        var candidates = await db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null)
            .Where(x => x.Attempts < maxAttempts)
            .Where(x => x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now)
            .Where(x => x.LockedUntilUtc == null || x.LockedUntilUtc <= now)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return [];

        var until = now.Add(lockDuration);
        foreach (var msg in candidates)
        {
            msg.LockedBy = _workerId;
            msg.LockedUntilUtc = until;
        }

        await db.SaveChangesAsync(ct);

        var ids = candidates.Select(x => x.Id).ToList();
        return await db.OutboxMessages
            .Where(x => ids.Contains(x.Id))
            .Where(x => x.LockedBy == _workerId)
            .ToListAsync(ct);
    }

    private int ComputeBackoffSeconds(int attempts)
    {
        var baseDelay = Math.Max(1, _options.BaseRetryDelaySeconds);
        var maxDelay = Math.Max(baseDelay, _options.MaxRetryDelaySeconds);

        var delay = baseDelay * (int)Math.Pow(2, Math.Max(0, attempts - 1));
        return Math.Min(delay, maxDelay);
    }
}
