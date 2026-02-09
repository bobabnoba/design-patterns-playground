using Microsoft.EntityFrameworkCore.Storage;
using Orders.Application.Abstractions;

namespace Orders.Infrastructure.Persistence;

public sealed class EfUnitOfWork(OrdersDbContext db) : IUnitOfWork
{
    private IDbContextTransaction? _tx;

    public async Task BeginAsync(CancellationToken ct)
        => _tx = await db.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);

        if (_tx is not null)
            await _tx.CommitAsync(ct);
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_tx is not null)
            await _tx.RollbackAsync(ct);
    }
}