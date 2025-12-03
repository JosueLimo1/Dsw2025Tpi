using Dsw2025Tpi.Api.DependencyInjection;
using Dsw2025Tpi.Api.Middleware;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Dsw2025Tpi.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================
            // JWT CONFIGURATION
            // ==========================

            var jwtConfig = builder.Configuration.GetSection("Jwt");
            var jwtKey = jwtConfig["Key"] ?? throw new Exception("Falta Jwt:Key en appsettings.json");
            var jwtIssuer = jwtConfig["Issuer"] ?? throw new Exception("Falta Jwt:Issuer en appsettings.json");
            var jwtAudience = jwtConfig["Audience"] ?? throw new Exception("Falta Jwt:Audience en appsettings.json");
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            // ==========================
            // SERVICES
            // ==========================

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dsw2025Tpi.Api", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Ingrese un token JWT en el encabezado como: Bearer {token}",
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
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            // ==========================
            // IDENTITY + JWT
            // ==========================

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<AuthenticateContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = signingKey,
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", // Estandariza el claim de Rol
                    ClockSkew = TimeSpan.Zero
                };

                // ----------------------------------------------------
                // AQUÍ ESTÁ LA CORRECCIÓN DEL ERROR "RESPONSE STARTED"
                // ----------------------------------------------------
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Si la respuesta ya empezó a enviarse, no hacemos nada para evitar el crash
                        if (context.Response.HasStarted)
                        {
                            return Task.CompletedTask;
                        }

                        context.NoResult();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "text/plain";
                        return context.Response.WriteAsync("Token inválido o expirado.");
                    },
                    OnChallenge = context =>
                    {
                        // Si la respuesta ya empezó, no intentamos escribir de nuevo
                        if (context.Response.HasStarted)
                        {
                            return Task.CompletedTask;
                        }

                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\": \"No autorizado. Token inválido o ausente.\"}");
                    }
                };
            });

            builder.Services.AddAuthorization();

            // ==========================
            // CUSTOM SERVICES & DB
            // ==========================

            // Asegúrate de que este método de extensión exista en tu proyecto, si no, comenta esta línea
            builder.Services.AddDomainServices(builder.Configuration);

            builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();

            builder.Services.AddDbContext<AuthenticateContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb"));
            });

            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            // ==========================
            // CORS
            // ==========================

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

            var app = builder.Build();

            app.UseCors("AllowAll");

            using (var scope = app.Services.CreateScope())
            {
                // Migraciones del contexto principal
                var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
                dbContext.Database.Migrate();

                // Si tienes un método seed, asegúrate que no duplique datos si ya existen
                try { dbContext.SeedDatabase(); } catch { /* Ignorar si ya existen datos */ }

                // Migraciones del contexto de autenticación
                var authContext = scope.ServiceProvider.GetRequiredService<AuthenticateContext>();
                authContext.Database.Migrate();

                // Inicialización de Roles y Admin
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var roles = builder.Configuration.GetSection("Roles").Get<List<string>>() ?? new() { "Admin", "User" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                var adminUsername = builder.Configuration["AdminUser:Username"];
                var adminEmail = builder.Configuration["AdminUser:Email"];
                var adminPassword = builder.Configuration["AdminUser:Password"];

                if (!string.IsNullOrEmpty(adminUsername) && await userManager.FindByNameAsync(adminUsername) == null)
                {
                    var adminUser = new IdentityUser
                    {
                        UserName = adminUsername,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }

            // ==========================
            // MIDDLEWARE HTTP
            // ==========================

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Middleware de excepciones personalizado
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}