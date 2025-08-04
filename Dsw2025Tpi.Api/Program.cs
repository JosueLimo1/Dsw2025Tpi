using Dsw2025Tpi.Api.Middleware;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Agrega los controladores al contenedor
        builder.Services.AddControllers();

        // Configuración JWT para autenticación
        // Se utiliza el esquema Bearer y se leen los valores desde appsettings.json
        var jwtConfig = builder.Configuration.GetSection("Jwt"); // Lee "Key", "Issuer", "Audience"
        var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]);      // Convierte la clave a bytes

        // Agrega el sistema de autenticación basado en tokens JWT
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig["Issuer"],
                ValidAudience = jwtConfig["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                RoleClaimType = ClaimTypes.Role // Permite usar [Authorize(Roles = "...")]
            };
        });

        // Herramientas para documentación y diagnóstico
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();

        // Configuración de bases de datos
        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MainDb")));

        builder.Services.AddDbContext<AuthenticateContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));

        // Registro de dependencias de servicios e interfaces
        builder.Services.AddScoped<IProductsManagementService, ProductsManagementService>();
        builder.Services.AddScoped<IRepository, EfRepository>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IOrdersManagementService, OrdersManagementService>();


        var app = builder.Build();

        // Sembrado inicial de la base de datos (carga de Customers desde JSON, por ejemplo)
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            db.SeedDatabase();
        }

        // Configuración del pipeline HTTP
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Manejo de excepciones personalizadas 
        app.UseMiddleware<CustomExceptionHandlerMiddleware>();

        // Habilita autenticación y autorización
        app.UseAuthentication();
        app.UseAuthorization();

        // Ruteo principal de controladores y endpoints de salud
        app.MapControllers();
        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}
