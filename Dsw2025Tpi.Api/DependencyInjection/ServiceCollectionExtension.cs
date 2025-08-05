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
    // Clase estática que extiende IServiceCollection para configurar los servicios de dominio
    public static class ServiceCollectionExtensions
    {
        // Método de extensión para registrar servicios y contexto de base de datos
        public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Obtiene la cadena de conexión desde appsettings.json (clave: MainDb)
            var connectionString = configuration.GetConnectionString("MainDb");

            // Registra la implementación del repositorio de datos (genérico)
            services.AddScoped<IRepository, EfRepository>();

            // Registra los servicios de productos y órdenes
            services.AddScoped<IProductsManagementService, ProductsManagementService>();
            services.AddScoped<IOrdersManagementService, OrdersManagementService>();

            // Registra el DbContext con SQL Server usando la cadena de conexión
            services.AddDbContext<Dsw2025TpiContext>(options =>
                options.UseSqlServer(connectionString));

            // Devuelve la colección de servicios con las dependencias registradas
            return services;
        }
    }
}
