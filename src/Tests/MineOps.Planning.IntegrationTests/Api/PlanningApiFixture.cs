using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MineOps.Planning.Infrastructure.Persistence;

namespace MineOps.Planning.IntegrationTests.Api;

public sealed class PlanningApiFixture : IAsyncLifetime
{
    private const string DatabasePrefix = "MineOpsPlanning_ApiTest_";
    private WebApplicationFactory<Program>? _factory;

    public PlanningApiFixture()
    {
        DatabaseName = $"{DatabasePrefix}{Guid.NewGuid():N}";
        ConnectionString = $"Server=localhost;Database={DatabaseName};Integrated Security=true;TrustServerCertificate=true";
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public HttpClient Client { get; private set; } = null!;

    public HttpClient CreateClient(string? token = null)
    {
        var client = _factory?.CreateClient()
            ?? throw new InvalidOperationException("The Planning test host has not started.");
        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:Planning", ConnectionString);
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Planning"] = ConnectionString
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<PlanningDbContext>>();
                services.RemoveAll<PlanningDbContext>();
                services.AddDbContext<PlanningDbContext>(options =>
                    options.UseSqlServer(ConnectionString));
            });
        });

        Client = CreateClient(Security.TestJwtTokens.Create("Administrator"));
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
        EnsureExpectedDatabase(dbContext);
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (!DatabaseName.StartsWith(DatabasePrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to delete an unexpected database.");
        }

        if (_factory is not null)
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
            EnsureExpectedDatabase(dbContext);
            await dbContext.Database.EnsureDeletedAsync();
            Client.Dispose();
            await _factory.DisposeAsync();
        }
    }

    private void EnsureExpectedDatabase(PlanningDbContext dbContext)
    {
        var configuredDatabase = dbContext.Database.GetDbConnection().Database;
        if (!string.Equals(configuredDatabase, DatabaseName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Refusing database operation: expected '{DatabaseName}', got '{configuredDatabase}'.");
        }
    }
}
