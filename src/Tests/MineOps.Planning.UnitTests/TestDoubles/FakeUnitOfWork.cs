using MineOps.Planning.Application.Abstractions;

namespace MineOps.Planning.UnitTests.TestDoubles;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCalls { get; private set; }

    public int TransactionCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCalls++;
        return Task.FromResult(1);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        TransactionCalls++;
        await operation(cancellationToken);
    }
}
