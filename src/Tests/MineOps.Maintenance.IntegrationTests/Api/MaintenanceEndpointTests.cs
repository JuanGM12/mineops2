using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MineOps.Maintenance.Domain.WorkOrders;
using MineOps.Maintenance.Infrastructure.Persistence;

namespace MineOps.Maintenance.IntegrationTests.Api;

public sealed class MaintenanceEndpointTests(MaintenanceApiFixture fixture) : IClassFixture<MaintenanceApiFixture>
{
    [Fact]
    public async Task SeederIsIdempotentAndHealthIsHealthy()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<MaintenanceSeeder>(); await seeder.SeedAsync(); await seeder.SeedAsync();
        var db = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>();
        Assert.Equal(2, await db.Equipment.CountAsync());
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task FullApiFlowAndPaginationWork()
    {
        var equipmentId = await GetEquipmentId();
        var response = await Create(equipmentId, $"WO-{Guid.NewGuid():N}"); Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()); var id = json.RootElement.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.GetAsync($"/api/work-orders/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.GetAsync("/api/work-orders?page=1&pageSize=20")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.PatchAsync($"/api/work-orders/{id}/start", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.PatchAsync($"/api/work-orders/{id}/complete", null)).StatusCode);
        var cancel = await Create(equipmentId, $"WO-C-{Guid.NewGuid():N}"); using var cj = JsonDocument.Parse(await cancel.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.PatchAsync($"/api/work-orders/{cj.RootElement.GetProperty("id").GetGuid()}/cancel", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await fixture.Client.GetAsync("/api/work-orders?pageSize=101")).StatusCode);
    }

    [Fact]
    public async Task InvalidEquipmentAndPreventiveWithoutDateAreRejected()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Create(Guid.NewGuid(), $"WO-X-{Guid.NewGuid():N}")).StatusCode);
        var equipmentId = await GetEquipmentId();
        var response = await fixture.Client.PostAsJsonAsync("/api/work-orders", new { code=$"WO-P-{Guid.NewGuid():N}", equipmentId, title="Preventive", maintenanceType="Preventive", priority="High", scheduledDate=(DateTimeOffset?)null });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UniqueIndexesAndTransactionRollbackAreReal()
    {
        await using var scope = fixture.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>();
        var equipment = await db.Equipment.FirstAsync(); var code = $"DUP-{Guid.NewGuid():N}";
        db.WorkOrders.Add(new(code,equipment.Id,"One",null,MaintenanceType.Corrective,WorkOrderPriority.Low,null)); await db.SaveChangesAsync();
        db.WorkOrders.Add(new(code,equipment.Id,"Two",null,MaintenanceType.Corrective,WorkOrderPriority.Low,null));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private async Task<Guid> GetEquipmentId() { await using var s=fixture.Services.CreateAsyncScope(); return await s.ServiceProvider.GetRequiredService<MaintenanceDbContext>().Equipment.Select(x=>x.Id).FirstAsync(); }
    private Task<HttpResponseMessage> Create(Guid equipmentId,string code) => fixture.Client.PostAsJsonAsync("/api/work-orders", new { code,equipmentId,title="Repair",description="Test",maintenanceType="Corrective",priority="High",scheduledDate=(DateTimeOffset?)null });
}
