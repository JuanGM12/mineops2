using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MineOps.Identity.Infrastructure.Identity;

namespace MineOps.Identity.IntegrationTests.Api;

public sealed class IdentityApiFixture : IAsyncLifetime
{
    private const string DatabasePrefix = "MineOpsIdentity_ApiTest_";
    public const string Password = "MineOps2026!";
    public const string SigningKey = "MineOps-Integration-Test-Signing-Key-2026-Only!";
    private WebApplicationFactory<Program>? _factory;

    public IdentityApiFixture()
    {
        DatabaseName = $"{DatabasePrefix}{Guid.NewGuid():N}";
        ConnectionString = $"Server=localhost;Database={DatabaseName};Integrated Security=true;Encrypt=false;TrustServerCertificate=true";
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public HttpClient Client { get; private set; } = null!;

    public IServiceProvider Services => _factory?.Services
        ?? throw new InvalidOperationException("The Identity test host has not started.");

    public async Task InitializeAsync()
    {
        await using (var migrationContext = CreateContext())
        {
            EnsureExpectedDatabase(migrationContext);
            await migrationContext.Database.MigrateAsync();
        }

        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Identity"] = ConnectionString,
            ["Jwt:Issuer"] = "MineOps.Identity",
            ["Jwt:Audience"] = "MineOps.Api",
            ["Jwt:SigningKey"] = SigningKey,
            ["Jwt:ExpirationMinutes"] = "60",
            ["Seed:DefaultPassword"] = Password
        };

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.UseEnvironment("Development");
            foreach (var setting in settings)
            {
                builder.UseSetting(setting.Key, setting.Value);
            }

            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(settings));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<MineOpsIdentityDbContext>>();
                services.RemoveAll<MineOpsIdentityDbContext>();
                services.AddDbContext<MineOpsIdentityDbContext>(options =>
                    options.UseSqlServer(ConnectionString));
            });
        });

        Client = _factory.CreateClient();
        await using var scope = Services.CreateAsyncScope();
        EnsureExpectedDatabase(scope.ServiceProvider.GetRequiredService<MineOpsIdentityDbContext>());
    }

    public async Task DisposeAsync()
    {
        if (!DatabaseName.StartsWith(DatabasePrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to delete an unexpected database.");
        }

        if (_factory is not null)
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MineOpsIdentityDbContext>();
            EnsureExpectedDatabase(dbContext);
            await dbContext.Database.EnsureDeletedAsync();
            Client.Dispose();
            await _factory.DisposeAsync();
        }
    }

    public MineOpsIdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MineOpsIdentityDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new MineOpsIdentityDbContext(options);
    }

    private void EnsureExpectedDatabase(MineOpsIdentityDbContext dbContext)
    {
        var configuredDatabase = dbContext.Database.GetDbConnection().Database;
        if (!string.Equals(configuredDatabase, DatabaseName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Refusing database operation: expected '{DatabaseName}', got '{configuredDatabase}'.");
        }
    }
}
