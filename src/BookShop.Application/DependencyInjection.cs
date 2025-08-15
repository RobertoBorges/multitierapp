using BookShop.Application.Interfaces;
using BookShop.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookShop.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
