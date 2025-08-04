using Dsw2025Tpi.Api.Middleware;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Win32;
using System.Data;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
namespace Dsw2025Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
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
        //builder.Services.AddSwaggerGen();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dsw2025Tpi.Api", Version = "v1" });

            // Configuración para el uso de JWT en Swagger UI
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                              "Ingresa 'Bearer' [espacio] y luego tu token.\r\n\r\n" +
                              "Ejemplo: \"Bearer abcdef12345\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });
        // Agrega soporte para Swagger y OpenAPI (ultimo cambio de program.cs)
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

        //Registro de Identity para autenticación y roles
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
       .AddEntityFrameworkStores<AuthenticateContext>()
       .AddDefaultTokenProviders();

        // Configuración de CORS para permitir todas las solicitudes
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        app.UseCors("AllowAll");


        // Sembrado inicial de la base de datos (carga de Customers desde JSON, por ejemplo)
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            db.SeedDatabase();
        }

        // === Crear roles y usuario admin por defecto ===
        using (var scope = app.Services.CreateScope())
        {
            // Servicios necesarios para trabajar con usuarios y roles
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // 1. Crear roles "Admin" y "User" si no existen
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Crear usuario administrador por defecto (configurado en appsettings.json)
            var adminUsername = config["AdminUser:Username"];
            var adminEmail = config["AdminUser:Email"];
            var adminPassword = config["AdminUser:Password"];

            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
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
