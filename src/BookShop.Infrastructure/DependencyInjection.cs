using System;
using BookShop.Core.Interfaces;
using BookShop.Infrastructure.Data;
using BookShop.Infrastructure.HealthChecks;
using BookShop.Infrastructure.Migrations;
using BookShop.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookShop.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<BookShopDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("BookShopDB"),
                    b => b.MigrationsAssembly(typeof(BookShopDbContext).Assembly.FullName)
                         .EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null))
                .EnableSensitiveDataLogging(false)
                .EnableDetailedErrors(false));

            // Add the migration startup filter for automatic migrations and DbInitializer
            services.AddTransient<IStartupFilter, MigrationStartupFilter>();
            services.AddTransient<DbInitializer>();

            // Register repositories
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            
            // Register health checks
            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database_health", 
                    failureStatus: HealthStatus.Degraded, 
                    tags: new[] { "database", "sql", "ready" });

            return services;
        }
    }
}
