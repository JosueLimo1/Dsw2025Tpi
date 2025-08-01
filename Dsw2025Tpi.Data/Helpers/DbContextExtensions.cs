using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Data.Helpers
{
    // Clase estática que extiende el DbContext con un método para sembrar datos
    public static class DbContextExtensions
    {
        // Configura el deserializador JSON para que no distinga mayúsculas y minúsculas en los nombres de propiedades
        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        // Método de extensión que carga datos iniciales desde archivos .json si la base de datos está vacía
        public static void SeedDatabase(this Dsw2025TpiContext context)
        {
            // --- 1. Clientes ---
            // Si no hay ningún cliente en la base, carga el archivo customers.json
            if (!context.Customers.Any())
            {
                // Lee el archivo de clientes desde la carpeta Sources
                var customersJson = File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dsw2025Tpi.Data", "Sources", "customers.json"));

                // Deserializa el JSON en una lista de objetos Customer
                var customers = JsonSerializer.Deserialize<List<Customer>>(customersJson, CachedJsonOptions);

                // Si hay clientes válidos, los agrega a la base de datos
                if (customers != null && customers.Count > 0)
                {
                    context.Customers.AddRange(customers);
                    context.SaveChanges();
                }
            }

            // --- 2. Productos ---
            // Si no hay productos en la base, carga el archivo products.json
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

            // --- 3. Órdenes ---
            // Si no hay órdenes en la base, carga el archivo orders.json
            if (!context.Orders.Any())
            {
                var ordersJson = File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dsw2025Tpi.Data", "Sources", "orders.json"));

                var orders = JsonSerializer.Deserialize<List<Order>>(ordersJson, CachedJsonOptions);

                if (orders != null && orders.Count > 0)
                {
                    // Se limpian los OrderItems para evitar errores de navegación al guardar
                    foreach (var order in orders)
                    {
                        order.OrderItems = new List<OrderItem>();
                    }

                    context.Orders.AddRange(orders);
                    context.SaveChanges();
                }
            }
        }
    }

}
