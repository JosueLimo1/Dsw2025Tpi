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

namespace Dsw2025Tpi.Api
{
    // Clase principal de arranque de la aplicaci�n ASP.NET Core
    public class Program
    {
        // M�todo de entrada asincr�nico
        // Este m�todo configura y ejecuta toda la app (registro de servicios, migraciones, middleware, etc.)
        public static async Task Main(string[] args)
        {
            // Crea el generador de la aplicaci�n web
            var builder = WebApplication.CreateBuilder(args);

            // ==========================
            // JWT CONFIGURATION
            // ==========================

            // Lee la configuraci�n de JWT desde appsettings.json
            var jwtConfig = builder.Configuration.GetSection("Jwt");

            // Obtiene la clave secreta para firmar los tokens JWT
            var jwtKey = jwtConfig["Key"] ?? throw new Exception("Falta Jwt:Key en appsettings.json");

            // Emisor del token (la API)
            var jwtIssuer = jwtConfig["Issuer"] ?? throw new Exception("Falta Jwt:Issuer en appsettings.json");

            // Audiencia esperada (el frontend u otro consumidor autorizado)
            var jwtAudience = jwtConfig["Audience"] ?? throw new Exception("Falta Jwt:Audience en appsettings.json");

            // Convierte la clave en una clave sim�trica para la firma del token
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            // ==========================
            // SERVICES
            // ==========================

            // Registra los controladores como endpoints RESTful
            builder.Services.AddControllers();

            // Habilita Swagger (documentaci�n de tu API)I
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                // Define la info b�sica del Swagger
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dsw2025Tpi.Api", Version = "v1" });

                // Agrega opci�n para ingresar el token JWT en Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Ingrese un token JWT en el encabezado como: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Esto le dice a Swagger que tu API necesita un token para funcionar
                // en los endpoints protegidos.
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

            // Agrega un servicio de monitoreo (ping de salud)
            //builder.Services.AddHealthChecks();

            // ==========================
            // IDENTITY + JWT
            // ==========================

            // Configura el sistema de autenticaci�n de usuarios (Identity) + roles
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Reglas m�nimas para contrase�as
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AuthenticateContext>() // Usa tu contexto de autenticaci�n
            .AddDefaultTokenProviders(); // Habilita generaci�n de tokens (reset pass, confirmaci�n, etc.)

            // Configura el sistema de autenticaci�n por JWT (Bearer)
            builder.Services.AddAuthentication(options =>
            { 
                // Establece JWT como el sistema de autenticaci�n predeterminado
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Permite HTTP (solo �til para testing local)
                options.SaveToken = true; // Guarda el token en el contexto del request

                // Configura c�mo se valida el token JWT
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
                    ClockSkew = TimeSpan.Zero // Sin margen de tiempo para expiraci�n
                };

                // Eventos personalizados para manejar errores de autenticaci�n
                options.Events = new JwtBearerEvents
                {
                    // Token inv�lido o expirado
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "text/plain";
                        return context.Response.WriteAsync("Token inv�lido o expirado.");
                    },
                    // Token ausente o mal formado
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\": \"Token requerido o inv�lido.\"}");
                    }
                };
            });

            // Habilita el sistema de autorizaci�n basado en roles
            builder.Services.AddAuthorization();

            // ==========================
            // CUSTOM SERVICES & DB
            // ==========================

            // Registra tus servicios personalizados y el contexto de dominio principal
            builder.Services.AddDomainServices(builder.Configuration);

            // Registra el contexto para autenticaci�n (usuarios + roles)
            builder.Services.AddDbContext<AuthenticateContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb"));
            });

            // Servicio para generar tokens JWT
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            // ==========================
            // CORS
            // ==========================

            // Configura CORS para permitir cualquier origen (�til en frontend local o testing)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            // ==========================
            // APP BUILD + MIGRACIONES
            // ==========================

            // Construye la aplicaci�n ASP.NET Core
            var app = builder.Build();

            // Aplica pol�tica de CORS
            app.UseCors("AllowAll");

            // Ejecuta migraciones y carga de datos iniciales
            using (var scope = app.Services.CreateScope())
            {
                // Aplica migraciones al contexto principal del dominio
                var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
                dbContext.Database.Migrate();
                dbContext.SeedDatabase(); // Carga clientes desde JSON

                // Aplica migraciones al contexto de autenticaci�n
                var authContext = scope.ServiceProvider.GetRequiredService<AuthenticateContext>();
                authContext.Database.Migrate();

                // Crea roles si no existen
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roles = builder.Configuration.GetSection("Roles").Get<List<string>>() ?? new() { "Admin", "User" };

                foreach (var role in roles) // Recorre cada rol de la lista (por ejemplo: "Admin", "User")
                {
                    if (!await roleManager.RoleExistsAsync(role)) // Si el rol todav�a no existe en la base de datos...
                    {
                        await roleManager.CreateAsync(new IdentityRole(role)); // ...lo crea usando Identity
                    }
                }


                // Crea usuario administrador si no existe
                var adminUsername = builder.Configuration["AdminUser:Username"];
                var adminEmail = builder.Configuration["AdminUser:Email"];
                var adminPassword = builder.Configuration["AdminUser:Password"];

                // Verifica si ya existe un usuario con el nombre de administrador configurado
                if (await userManager.FindByNameAsync(adminUsername) == null)
                {
                    // Si no existe, crea un nuevo objeto IdentityUser con los datos del admin
                    var adminUser = new IdentityUser
                    {
                        UserName = adminUsername,     // Nombre de usuario del admin (por ejemplo, "admin")
                        Email = adminEmail,           // Email del admin (por ejemplo, "admin@miapp.com")
                        EmailConfirmed = true         // Marca el email como ya confirmado
                    };

                    // Intenta crear el usuario en la base de datos con la contrase�a configurada
                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    // Si la creaci�n fue exitosa...
                    if (result.Succeeded)
                    {
                        // ...le asigna el rol "Admin" al nuevo usuario
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

            }

            // ==========================
            // MIDDLEWARE HTTP
            // ==========================

            // Habilita Swagger solo en entorno de desarrollo
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Redirige autom�ticamente de HTTP a HTTPS
            app.UseHttpsRedirection();

            // Middleware global para manejo de excepciones personalizadas
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();

            // Habilita autenticaci�n JWT
            app.UseAuthentication();

            // Habilita autorizaci�n por roles
            app.UseAuthorization();

            // Mapea todos los controladores
            app.MapControllers();

            // Endpoint de verificaci�n de salud para monitoreo
            //app.MapHealthChecks("/healthcheck");

            // Inicia la aplicaci�n web
            app.Run();
        }
    }
}

