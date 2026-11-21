using Microsoft.EntityFrameworkCore;
using MineOps.Maintenance.Application.Abstractions;

namespace MineOps.Maintenance.Infrastructure.Persistence;

public sealed class EfUnitOfWork(MaintenanceDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken token) => db.SaveChangesAsync(token);
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken token)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(token);
            await operation(token);
            await transaction.CommitAsync(token);
        });
    }
}
