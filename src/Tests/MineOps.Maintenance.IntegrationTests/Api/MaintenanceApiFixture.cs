using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MineOps.Maintenance.Infrastructure.Persistence;

namespace MineOps.Maintenance.IntegrationTests.Api;

public sealed class MaintenanceApiFixture : IAsyncLifetime
{
    private const string Prefix = "MineOpsMaintenance_ApiTest_";
    private WebApplicationFactory<Program>? _factory;
    public string DatabaseName { get; } = $"{Prefix}{Guid.NewGuid():N}";
    public string ConnectionString => $"Server=localhost;Database={DatabaseName};Integrated Security=true;TrustServerCertificate=true";
    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _factory!.Services;

    public HttpClient CreateClient(string? token = null)
    {
        var client = _factory?.CreateClient()
            ?? throw new InvalidOperationException("The Maintenance test host has not started.");
        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<MaintenanceDbContext>().UseSqlServer(ConnectionString).Options;
        await using (var db = new MaintenanceDbContext(options)) { Ensure(db); await db.Database.MigrateAsync(); }
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.UseEnvironment("Development"); builder.UseSetting("ConnectionStrings:Maintenance", ConnectionString);
            builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string,string?> { ["ConnectionStrings:Maintenance"] = ConnectionString }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<MaintenanceDbContext>>(); services.RemoveAll<MaintenanceDbContext>();
                services.AddDbContext<MaintenanceDbContext>(o => o.UseSqlServer(ConnectionString));
            });
        });
        Client = CreateClient(Security.TestJwtTokens.Create("Administrator"));
    }

    public async Task DisposeAsync()
    {
        if (!DatabaseName.StartsWith(Prefix, StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe database name.");
        await using var scope = Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>(); Ensure(db);
        await db.Database.EnsureDeletedAsync(); Client.Dispose(); await _factory!.DisposeAsync();
    }

    private void Ensure(MaintenanceDbContext db)
    {
        if (db.Database.GetDbConnection().Database != DatabaseName) throw new InvalidOperationException("Refusing unsafe database operation.");
    }
}
