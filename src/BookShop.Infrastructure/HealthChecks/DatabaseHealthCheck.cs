using BookShop.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookShop.Infrastructure.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly BookShopDbContext _dbContext;

        public DatabaseHealthCheck(BookShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if the database connection is working
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Database connection is healthy");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Could not connect to the database");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Database connection error: {ex.Message}", ex);
            }
        }
    }
}
