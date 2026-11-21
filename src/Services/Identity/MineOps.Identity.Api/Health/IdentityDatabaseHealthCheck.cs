using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MineOps.Identity.Infrastructure.Identity;

namespace MineOps.Identity.Api.Health;

public sealed class IdentityDatabaseHealthCheck(MineOpsIdentityDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Identity database is available.")
                : HealthCheckResult.Unhealthy("Identity database is unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Identity database is unavailable.", exception);
        }
    }
}
