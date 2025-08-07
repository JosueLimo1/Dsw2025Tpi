using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dsw2025Tpi.Api.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MainDb");

            services.AddScoped<IRepository, EfRepository>();
            services.AddScoped<IProductsManagementService, ProductsManagementService>();
            services.AddScoped<IOrdersManagementService, OrdersManagementService>();

            services.AddDbContext<Dsw2025TpiContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
