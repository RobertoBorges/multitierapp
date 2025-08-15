using BookShop.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookShop.Infrastructure.Migrations
{
    public class MigrationStartupFilter : IStartupFilter
    {
        private readonly ILogger<MigrationStartupFilter> _logger;

        public MigrationStartupFilter(ILogger<MigrationStartupFilter> logger)
        {
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    try
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<BookShopDbContext>();
                        _logger.LogInformation("Checking database connection...");
                        
                        // Instead of migrating, just verify we can connect
                        if (dbContext.Database.CanConnect())
                        {
                            _logger.LogInformation("Successfully connected to the existing database.");
                            
                            // Initialize the database with seed data (our modified version just logs)
                            _logger.LogInformation("Initializing database connection...");
                            var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                            Task.Run(() => initializer.InitializeAsync()).Wait();
                        }
                        else
                        {
                            _logger.LogWarning("Cannot connect to database.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while applying migrations or initializing data: {Message}", ex.Message);
                        throw;
                    }
                }

                next(app);
            };
        }
    }
}
