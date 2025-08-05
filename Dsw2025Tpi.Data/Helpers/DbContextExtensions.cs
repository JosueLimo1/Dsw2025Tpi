using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// Define el namespace del proyecto donde se ubican los helpers del contexto de datos
namespace Dsw2025Tpi.Data.Helpers
{
    // Clase estática que contiene métodos de extensión para el DbContext
    public static class DbContextExtensions
    {
        // Opciones de configuración del deserializador JSON
        // Indica que los nombres de propiedades no deben diferenciar entre mayúsculas/minúsculas
        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        // Método de extensión que se aplica sobre una instancia del contexto
        // Permite cargar datos iniciales (semilla) si las tablas están vacías
        public static void SeedDatabase(this Dsw2025TpiContext context)
        {
            // ---------- CARGA DE CLIENTES (Customers) ----------

            // Verifica si hay clientes cargados en la base
            if (!context.Customers.Any())
            {
                // Lee el contenido del archivo customers.json desde la carpeta Sources
                var customersJson = File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dsw2025Tpi.Data", "Sources", "customers.json"));

                // Deserializa el JSON a una lista de objetos Customer
                var customers = JsonSerializer.Deserialize<List<Customer>>(customersJson, CachedJsonOptions);

                // Si hay clientes válidos, los agrega a la base
                if (customers != null && customers.Count > 0)
                {
                    context.Customers.AddRange(customers);
                    context.SaveChanges(); // Guarda los cambios en la base
                }
            }

            // ---------- BLOQUE COMENTADO: CARGA DE PRODUCTOS Y ÓRDENES ----------

            /*
            // Carga productos si la tabla está vacía
            if (!context.Products.Any())
            {
                var productsJson = File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dsw2025Tpi.Data", "Sources", "products.json"));

                var products = JsonSerializer.Deserialize<List<Product>>(productsJson, CachedJsonOptions);

                if (products != null && products.Count > 0)
                {
                    context.Products.AddRange(products);
                    context.SaveChanges();
                }
            }

            // Carga órdenes (sin ítems) si la tabla está vacía
            if (!context.Orders.Any())
            {
                var ordersJson = File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dsw2025Tpi.Data", "Sources", "orders.json"));

                var orders = JsonSerializer.Deserialize<List<Order>>(ordersJson, CachedJsonOptions);

                if (orders != null && orders.Count > 0)
                {
                    // Limpia los OrderItems de cada orden para evitar referencias nulas
                    foreach (var order in orders)
                    {
                        order.OrderItems = new List<OrderItem>();
                    }

                    context.Orders.AddRange(orders);
                    context.SaveChanges();
                }
            }
            */
        }
    }
}
