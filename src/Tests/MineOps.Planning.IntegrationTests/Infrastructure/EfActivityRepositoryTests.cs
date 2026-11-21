using Microsoft.EntityFrameworkCore;
using MineOps.Planning.Domain.Activities;
using MineOps.Planning.Infrastructure.Persistence;
using MineOps.Planning.Infrastructure.Persistence.Repositories;

namespace MineOps.Planning.IntegrationTests.Infrastructure;

public sealed class EfActivityRepositoryTests(SqlServerDatabaseFixture fixture)
    : IClassFixture<SqlServerDatabaseFixture>
{
    [Fact]
    public async Task RepositoryPersistsAndReadsActivity()
    {
        await using var dbContext = fixture.CreateContext();
        var repository = new EfActivityRepository(dbContext);
        var unitOfWork = new EfUnitOfWork(dbContext);
        var activity = Create($"ACT-{Guid.NewGuid():N}");

        await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
        {
            await repository.AddAsync(activity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }, CancellationToken.None);

        dbContext.ChangeTracker.Clear();
        var persisted = await repository.GetByIdAsync(activity.Id, CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal(activity.Code, persisted.Code);
        Assert.Equal(TimeSpan.Zero, persisted.ScheduledDate.Offset);
    }

    [Fact]
    public async Task UniqueIndexRejectsDuplicateCode()
    {
        await using var dbContext = fixture.CreateContext();
        var code = $"DUP-{Guid.NewGuid():N}";
        dbContext.Activities.Add(Create(code));
        await dbContext.SaveChangesAsync();
        dbContext.Activities.Add(Create(code));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task TransactionRollsBackWhenOperationFails()
    {
        var code = $"ROLLBACK-{Guid.NewGuid():N}";

        await using (var dbContext = fixture.CreateContext())
        {
            var repository = new EfActivityRepository(dbContext);
            var unitOfWork = new EfUnitOfWork(dbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
                {
                    await repository.AddAsync(Create(code), cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    throw new InvalidOperationException("Force rollback.");
                }, CancellationToken.None));
        }

        await using var verificationContext = fixture.CreateContext();
        Assert.False(await verificationContext.Activities.AnyAsync(activity => activity.Code == code));
    }

    [Fact]
    public async Task RepositoryFiltersPendingOverdueAndResponsibleActivities()
    {
        var responsibleId = Guid.NewGuid();
        var overdue = Create(
            $"OVERDUE-{Guid.NewGuid():N}",
            responsibleId,
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1));
        var started = Create($"STARTED-{Guid.NewGuid():N}");
        started.Start();

        await using var dbContext = fixture.CreateContext();
        dbContext.Activities.AddRange(overdue, started);
        await dbContext.SaveChangesAsync();
        var repository = new EfActivityRepository(dbContext);

        var pending = await repository.GetPendingAsync(1, 100, CancellationToken.None);
        var overduePage = await repository.GetOverdueAsync(
            DateTimeOffset.UtcNow,
            1,
            100,
            CancellationToken.None);
        var responsible = await repository.GetByResponsibleAsync(
            responsibleId,
            1,
            100,
            CancellationToken.None);

        Assert.Contains(pending.Items, activity => activity.Id == overdue.Id);
        Assert.Contains(overduePage.Items, activity => activity.Id == overdue.Id);
        Assert.Single(responsible.Items);
        Assert.Equal(overdue.Id, responsible.Items[0].Id);
    }

    private static Activity Create(
        string code,
        Guid? responsibleId = null,
        DateTimeOffset? scheduledDate = null,
        DateTimeOffset? dueDate = null) => new(
            code,
            "Integration activity",
            null,
            "Operations",
            "Mine",
            responsibleId,
            ActivityPriority.High,
            scheduledDate ?? DateTimeOffset.UtcNow.AddDays(1),
            dueDate ?? DateTimeOffset.UtcNow.AddDays(2));
}
