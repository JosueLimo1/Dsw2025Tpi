using Dsw2025Tpi.Api.DependencyInjection;
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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

// Namespace principal del proyecto web/API
namespace Dsw2025Tpi.Api;

// Clase principal de arranque de la aplicaci�n ASP.NET Core
public class Program
{
    // M�todo de entrada asincr�nico (permite tareas como la creaci�n de usuarios)
    public static async Task Main(string[] args)
    {
        // Crea el generador de la aplicaci�n web (con DI, config, etc.)
        var builder = WebApplication.CreateBuilder(args);

        // ==========================
        // JWT CONFIGURATION
        // ==========================

        // Obtiene la secci�n Jwt del appsettings.json
        var jwtConfig = builder.Configuration.GetSection("Jwt");

        // Recupera la clave secreta para firmar tokens JWT
        var jwtKey = jwtConfig["Key"] ?? throw new Exception("Falta Jwt:Key en appsettings.json");

        // Recupera el emisor del token
        var jwtIssuer = jwtConfig["Issuer"] ?? throw new Exception("Falta Jwt:Issuer en appsettings.json");

        // Recupera la audiencia permitida para el token
        var jwtAudience = jwtConfig["Audience"] ?? throw new Exception("Falta Jwt:Audience en appsettings.json");

        // Crea una clave de firma sim�trica basada en la clave secreta
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        // ==========================
        // SERVICES
        // ==========================

        // Registra los controladores de la API
        builder.Services.AddControllers();

        // Agrega el explorador de endpoints (para Swagger)
        builder.Services.AddEndpointsApiExplorer();

        // Configura Swagger para documentaci�n interactiva de la API
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dsw2025Tpi.Api", Version = "v1" });

            // Agrega esquema de seguridad tipo Bearer para JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Ingrese un token JWT v�lido con el esquema 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Define el requerimiento de seguridad para aplicar a los endpoints
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
                        Scheme = "Bearer",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        // Agrega el servicio de health checks (verificaci�n de salud de la app)
        builder.Services.AddHealthChecks();

        // ==========================
        // IDENTITY + JWT
        // ==========================

        // Configura Identity para gesti�n de usuarios, contrase�as y roles
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<AuthenticateContext>() // Usa AuthenticateContext como base de datos
        .AddDefaultTokenProviders(); // Activa generaci�n de tokens para recuperaci�n de contrase�a, etc.

        // Configura el esquema de autenticaci�n por defecto como JWT Bearer
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // No requiere HTTPS (�til para testing local)
            options.SaveToken = true; // Guarda el token en el contexto

            // Par�metros de validaci�n del token
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = signingKey,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.Zero // No permite gracia en expiraci�n (vence justo en la hora)
            };

            // Eventos personalizados para errores en autenticaci�n
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    context.NoResult();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "text/plain";
                    return context.Response.WriteAsync("Token inv�lido o expirado.");
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync("{\"error\": \"Token requerido o inv�lido.\"}");
                }
            };
        });

        // Agrega el servicio de autorizaci�n (basado en roles o claims)
        builder.Services.AddAuthorization();

        // ==========================
        // CUSTOM SERVICES & DB
        // ==========================

        // Registra todos los servicios del dominio (productos, �rdenes, etc.)
        builder.Services.AddDomainServices(builder.Configuration);

        // Registra el contexto de autenticaci�n (Identity)
        builder.Services.AddDbContext<AuthenticateContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb"));
        });

        // Servicio encargado de generar tokens JWT
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ==========================
        // CORS
        // ==========================

        // Habilita CORS para permitir cualquier origen, m�todo y header
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        // Construye la aplicaci�n
        var app = builder.Build();

        // Aplica la pol�tica de CORS
        app.UseCors("AllowAll");

        // ==========================
        // DB SEED + ROLES
        // ==========================

        // Crea un scope para servicios que se usan temporalmente
        using (var scope = app.Services.CreateScope())
        {
            // Obtiene el contexto de dominio y aplica migraciones pendientes
            var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            dbContext.Database.Migrate();
            dbContext.SeedDatabase(); // Ejecuta el m�todo de carga inicial de datos (clientes)

            // Aplica migraciones tambi�n al contexto de autenticaci�n
            var authContext = scope.ServiceProvider.GetRequiredService<AuthenticateContext>();
            authContext.Database.Migrate();

            // Recupera servicios de Identity para roles y usuarios
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Obtiene la lista de roles a crear desde configuraci�n
            var roles = builder.Configuration.GetSection("Roles").Get<List<string>>() ?? new() { "Admin", "User" };

            // Crea los roles si a�n no existen
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Obtiene credenciales del usuario administrador desde configuraci�n
            var adminUsername = builder.Configuration["AdminUser:Username"];
            var adminEmail = builder.Configuration["AdminUser:Email"];
            var adminPassword = builder.Configuration["AdminUser:Password"];

            // Crea el usuario administrador si no existe
            if (await userManager.FindByNameAsync(adminUsername) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                // Si la creaci�n fue exitosa, le asigna el rol de "Admin"
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        // ==========================
        // MIDDLEWARE
        // ==========================

        // Si el entorno es de desarrollo, habilita Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Redirige HTTP a HTTPS autom�ticamente
        app.UseHttpsRedirection();

        // Middleware global para capturar excepciones y formatearlas
        app.UseMiddleware<CustomExceptionHandlerMiddleware>();

        // Habilita autenticaci�n JWT
        app.UseAuthentication();

        // Habilita autorizaci�n por roles
        app.UseAuthorization();

        // Mapea todos los controladores (endpoints REST)
        app.MapControllers();

        // Endpoint de health check para monitoreo
        app.MapHealthChecks("/healthcheck");

        // Inicia la aplicaci�n
        app.Run();
    }
}
