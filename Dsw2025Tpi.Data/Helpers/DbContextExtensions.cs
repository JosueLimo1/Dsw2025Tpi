using Dsw2025Tpi.Domain.Entities;
using System.Text.Json;

namespace Dsw2025Tpi.Data.Helpers
{
    public static class DbContextExtensions
    {
        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static void SeedDatabase(this Dsw2025TpiContext context)
        {
            if (!context.Customers.Any())
            {
                var customersJson = File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dsw2025Tpi.Data", "Sources", "customers.json"));

                var customers = JsonSerializer.Deserialize<List<Customer>>(customersJson, CachedJsonOptions);

                if (customers != null && customers.Count > 0)
                {
                    context.Customers.AddRange(customers);
                    context.SaveChanges();
                }
            }

            /*
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

            if (!context.Orders.Any())
            {
                var ordersJson = File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dsw2025Tpi.Data", "Sources", "orders.json"));

                var orders = JsonSerializer.Deserialize<List<Order>>(ordersJson, CachedJsonOptions);

                if (orders != null && orders.Count > 0)
                {
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
