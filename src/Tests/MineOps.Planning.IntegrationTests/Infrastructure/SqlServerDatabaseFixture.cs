using Microsoft.EntityFrameworkCore;
using MineOps.Planning.Infrastructure.Persistence;

namespace MineOps.Planning.IntegrationTests.Infrastructure;

public sealed class SqlServerDatabaseFixture : IAsyncLifetime
{
    private const string DatabasePrefix = "MineOpsPlanning_Test_";

    public SqlServerDatabaseFixture()
    {
        DatabaseName = $"{DatabasePrefix}{Guid.NewGuid():N}";
        ConnectionString = $"Server=localhost;Database={DatabaseName};Integrated Security=true;TrustServerCertificate=true";
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public PlanningDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlanningDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new PlanningDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (!DatabaseName.StartsWith(DatabasePrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to delete an unexpected database.");
        }

        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureDeletedAsync();
    }
}
