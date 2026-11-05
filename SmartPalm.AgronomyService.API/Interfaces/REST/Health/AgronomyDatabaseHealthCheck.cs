using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartPalm.AgronomyService.Infrastructure.Persistence;
namespace SmartPalm.AgronomyService.Api;
public class AgronomyDatabaseHealthCheck(AgronomyDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default) =>
        await db.Database.CanConnectAsync(token) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Agronomy database is unavailable.");
}
