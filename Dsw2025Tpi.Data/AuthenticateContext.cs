using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

// Namespace del contexto que maneja autenticación y roles
namespace Dsw2025Tpi.Data
{
    // Clase que representa el contexto específico para Identity (usuarios, roles, claims, etc.)
    // Hereda de IdentityDbContext que ya tiene la estructura de tablas necesaria para ASP.NET Identity
    public class AuthenticateContext : IdentityDbContext
    {
        // Constructor que recibe opciones de configuración desde Program.cs
        public AuthenticateContext(DbContextOptions<AuthenticateContext> options)
            : base(options)
        {
        }

        // Permite redefinir el modelo generado por Identity
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Llama a la configuración por defecto de Identity

            // Personaliza el nombre de cada tabla usada por Identity

            builder.Entity<IdentityUser>(b =>
            {
                b.ToTable("Usuarios"); // Cambia la tabla por defecto de AspNetUsers a Usuarios
            });

            builder.Entity<IdentityRole>(b =>
            {
                b.ToTable("Roles"); // Cambia AspNetRoles por Roles
            });

            builder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("UsuariosRoles"); // Cambia AspNetUserRoles por UsuariosRoles
            });

            builder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("UsuariosClaims"); // Reemplaza AspNetUserClaims
            });

            builder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("UsuariosLogins"); // Reemplaza AspNetUserLogins
            });

            builder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("RolesClaims"); // Reemplaza AspNetRoleClaims
            });

            builder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("UsuariosTokens"); // Reemplaza AspNetUserTokens
            });
        }
    }
}
