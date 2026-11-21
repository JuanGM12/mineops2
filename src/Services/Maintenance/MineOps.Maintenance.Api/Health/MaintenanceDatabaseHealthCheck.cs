using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MineOps.Maintenance.Infrastructure.Persistence;

namespace MineOps.Maintenance.Api.Health;

public sealed class MaintenanceDatabaseHealthCheck(MaintenanceDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
    {
        try { return await db.Database.CanConnectAsync(token) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy(); }
        catch (Exception e) { return HealthCheckResult.Unhealthy("Maintenance database unavailable.", e); }
    }
}
