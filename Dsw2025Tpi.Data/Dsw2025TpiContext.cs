// Importa las entidades del dominio
using Dsw2025Tpi.Domain.Entities;

// Importa herramientas de Entity Framework Core
using Microsoft.EntityFrameworkCore;

// Define el namespace donde se encuentra el DbContext principal
namespace Dsw2025Tpi.Data;

// Clase que representa el contexto de base de datos principal
public class Dsw2025TpiContext : DbContext
{
    // Constructor que recibe opciones de configuración desde Program.cs (inyección de dependencias)
    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {
    }

    // Definición de DbSet: representan tablas en la base de datos
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Customer> Customers { get; set; }

    // Método que se ejecuta al momento de construir el modelo de datos (tablas, columnas, restricciones)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Configuración para la entidad Customer ---
        modelBuilder.Entity<Customer>(eb =>
        {
            eb.ToTable("Customers"); // Nombre de la tabla

            eb.Property(c => c.Id)
              .ValueGeneratedNever(); // El ID se proporciona manualmente, no lo genera la base

            eb.Property(c => c.Email)
              .HasMaxLength(320); // Longitud máxima del email

            eb.Property(c => c.Name)
              .HasMaxLength(60)   // Nombre de hasta 60 caracteres
              .IsRequired();      // Obligatorio

            eb.Property(c => c.PhoneNumber); // Teléfono, sin validación adicional
        });

        // --- Configuración para la entidad Order ---
        modelBuilder.Entity<Order>(eb =>
        {
            eb.ToTable("Orders");

            eb.Property(o => o.Id)
              .ValueGeneratedNever();

            eb.Property(o => o.Date)
              .HasMaxLength(10)   // Longitud de la fecha como texto (aunque debería ser DateTime)
              .IsRequired();

            eb.Property(o => o.ShippingAddress)
              .HasMaxLength(60);  // Dirección de envío

            eb.Property(o => o.BillingAddress)
              .HasPrecision(15, 2); // ❗Esto parece un error: las direcciones son strings, y Precision es para decimales

            eb.Property(o => o.Notes)
              .HasMaxLength(60); // Notas opcionales
        });

        // --- Configuración para la entidad OrderItem ---
        modelBuilder.Entity<OrderItem>(eb =>
        {
            eb.ToTable("OrderItems");

            eb.Property(oi => oi.Id)
              .ValueGeneratedNever(); // ID manual

            eb.Property(oi => oi.Quantity)
              .IsRequired(); // Cantidad obligatoria

            eb.Property(oi => oi.UnitPrice)
              .HasPrecision(15, 2) // Precio con precisión decimal
              .IsRequired();      // Obligatorio
        });

        // --- Configuración para la entidad Product ---
        modelBuilder.Entity<Product>(eb =>
        {
            eb.ToTable("Products");

            eb.Property(p => p.Id)
              .ValueGeneratedNever(); // ID manual

            eb.Property(p => p.Sku)
              .HasMaxLength(20)
              .IsRequired(); // SKU obligatorio y de hasta 20 caracteres

            eb.Property(p => p.Name)
              .HasMaxLength(60)
              .IsRequired(); // Nombre obligatorio

            eb.Property(p => p.CurrentUnitPrice)
              .HasPrecision(15, 2); // Precio con precisión decimal

            eb.Property(p => p.InternalCode)
              .HasMaxLength(60); // Código interno de hasta 60 caracteres

            eb.Property(p => p.Description)
              .HasMaxLength(200); // Descripción de hasta 200 caracteres

            eb.Property(p => p.StockQuantity)
              .HasDefaultValue(0); // El stock por defecto es 0
        });
    }
}
