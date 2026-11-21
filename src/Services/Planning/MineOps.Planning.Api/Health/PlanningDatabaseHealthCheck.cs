using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MineOps.Planning.Infrastructure.Persistence;

namespace MineOps.Planning.Api.Health;

public sealed class PlanningDatabaseHealthCheck(PlanningDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Planning database is available.")
                : HealthCheckResult.Unhealthy("Planning database is unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Planning database is unavailable.", exception);
        }
    }
}
