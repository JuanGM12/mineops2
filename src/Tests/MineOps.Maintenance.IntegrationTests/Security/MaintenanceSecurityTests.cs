using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MineOps.Maintenance.Infrastructure.Persistence;
using MineOps.Maintenance.IntegrationTests.Api;

namespace MineOps.Maintenance.IntegrationTests.Security;

public sealed class MaintenanceSecurityTests(MaintenanceApiFixture fixture)
    : IClassFixture<MaintenanceApiFixture>
{
    [Fact]
    public async Task HealthIsAnonymousButWorkOrdersRequireToken()
    {
        using var client = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/work-orders")).StatusCode);
    }

    [Fact]
    public async Task ValidTokenIsAcceptedAndTamperedOrExpiredTokenReturns401()
    {
        var valid = TestJwtTokens.Create("Operator");
        using var validClient = fixture.CreateClient(valid);
        using var tamperedClient = fixture.CreateClient(TestJwtTokens.Tamper(valid));
        using var expiredClient = fixture.CreateClient(TestJwtTokens.Create("Operator", expired: true));

        Assert.Equal(HttpStatusCode.OK, (await validClient.GetAsync("/api/work-orders")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await tamperedClient.GetAsync("/api/work-orders")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await expiredClient.GetAsync("/api/work-orders")).StatusCode);
    }

    [Fact]
    public async Task MaintenanceSupervisorCreatesOrderButPlannerGets403()
    {
        var equipmentId = await EquipmentIdAsync();
        using var supervisor = fixture.CreateClient(TestJwtTokens.Create("MaintenanceSupervisor"));
        using var planner = fixture.CreateClient(TestJwtTokens.Create("Planner"));

        Assert.Equal(HttpStatusCode.Created,
            (await supervisor.PostAsJsonAsync("/api/work-orders", OrderBody(equipmentId, $"SEC-{Guid.NewGuid():N}"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await planner.PostAsJsonAsync("/api/work-orders", OrderBody(equipmentId, $"DENY-{Guid.NewGuid():N}"))).StatusCode);
    }

    [Fact]
    public async Task OperatorCanReadButCannotAdministerMaintenance()
    {
        var equipmentId = await EquipmentIdAsync();
        using var operatorClient = fixture.CreateClient(TestJwtTokens.Create("Operator"));
        Assert.Equal(HttpStatusCode.OK, (await operatorClient.GetAsync("/api/work-orders")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await operatorClient.PostAsJsonAsync("/api/work-orders", OrderBody(equipmentId, $"OP-{Guid.NewGuid():N}"))).StatusCode);
    }

    [Fact]
    public async Task SwaggerExposesBearerSchemeAndRoleDocumentation()
    {
        using var client = fixture.CreateClient();
        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("bearer", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MaintenanceSupervisor", json, StringComparison.Ordinal);
    }

    private async Task<Guid> EquipmentIdAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>()
            .Equipment.Select(x => x.Id).FirstAsync();
    }

    private static object OrderBody(Guid equipmentId, string code) => new
    {
        code,
        equipmentId,
        title = "Security test order",
        description = "JWT integration test",
        maintenanceType = "Corrective",
        priority = "High",
        scheduledDate = (DateTimeOffset?)null
    };
}
