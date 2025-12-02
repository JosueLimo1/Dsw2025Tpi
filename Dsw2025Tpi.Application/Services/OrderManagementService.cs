using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Validation;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService : IOrdersManagementService
    {
        private readonly Dsw2025TpiContext _context;

        public OrdersManagementService(Dsw2025TpiContext context)
        {
            _context = context;
        }

        // =================================================================
        // CREATE ORDER
        // =================================================================
        public async Task<OrderModel.ResponseOrderModel> CreateOrder(OrderModel.RequestOrderModel request)
        {
            OrderValidator.Validate(request);

            var customer = await _context.Customers.FindAsync(request.CustomerId);
            CustomerValidator.Validate(customer);

            var order = new Order(
                DateTime.UtcNow,
                request.ShippingAddress,
                request.BillingAddress,
                null,
                request.CustomerId)
            {
                Id = Guid.NewGuid()
            };

            foreach (var item in request.OrderItems)
            {
                OrderItemValidator.Validate(item);
                var product = await _context.Products.FindAsync(item.ProductId);
                ProductValidator.Validate(product!);
                ProductValidator.EnsureStock(product!, item.Quantity);

                order.AddItem(product, item.Quantity);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // CORRECCIÓN: Pasamos todos los campos nuevos al constructor
            return new OrderModel.ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.TotalAmount,     // Nuevo: Total
                order.CustomerId,
                customer.Name,         // Nuevo: Nombre Cliente
                order.Status,
                new List<OrderItemModel.ResponseOrderItemModel>() // Lista vacía (o mapeada si prefieres)
            );
        }

        // =================================================================
        // GET ALL ORDERS
        // =================================================================
        public async Task<IEnumerable<OrderModel.ResponseOrderModel>> GetAllOrders(OrderFilterModel? filter = null)
        {
            var query = _context.Orders
                .Include(o => o.Customer) // Importante para el nombre
                .Include(o => o.OrderItems) // Importante para calcular el TotalAmount
                .AsQueryable();

            if (filter is not null)
            {
                if (filter.CustomerId.HasValue)
                    query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

                if (filter.Status.HasValue)
                    query = query.Where(o => o.Status == filter.Status.Value);
            }

            var orders = await query.ToListAsync();

            // CORRECCIÓN: Mapeo completo
            return orders.Select(o => new OrderModel.ResponseOrderModel(
                o.Id,
                o.Date,
                o.ShippingAddress,
                o.BillingAddress,
                o.Notes,
                o.TotalAmount,                  // Nuevo
                o.CustomerId,
                o.Customer?.Name ?? "Desconocido", // Nuevo
                o.Status,
                new List<OrderItemModel.ResponseOrderItemModel>() // Lista vacía para no sobrecargar el listado
            ));
        }

        // =================================================================
        // GET ORDER BY ID
        // =================================================================
        public async Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Para nombre del producto
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return null;

            // Mapeo de Items para el detalle
            var itemsDto = order.OrderItems.Select(i => new OrderItemModel.ResponseOrderItemModel(
                i.Id,
                i.Quantity,
                i.UnitPrice,
                i.Subtotal, // Asegúrate que tu DTO de item tenga Subtotal, si no bórralo de aquí
                i.OrderId,
                i.ProductId,
                i.Product?.Name ?? "Producto no disponible" // Nombre del producto
            )).ToList();

            // CORRECCIÓN: Retorno completo
            return new OrderModel.ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.TotalAmount,              // Nuevo
                order.CustomerId,
                order.Customer?.Name ?? "Desconocido", // Nuevo
                order.Status,
                itemsDto                        // La lista de productos real
            );
        }

        // =================================================================
        // UPDATE STATUS
        // =================================================================
        public async Task<OrderModel.ResponseOrderModel?> UpdateOrderStatus(Guid id, OrderStatus newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)       // Necesitamos el cliente
                .Include(o => o.OrderItems)     // Necesitamos items para el total
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return null;

            order.ChangeStatus(newStatus);
            await _context.SaveChangesAsync();

            // CORRECCIÓN: Retorno completo
            return new OrderModel.ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.TotalAmount,              // Nuevo
                order.CustomerId,
                order.Customer?.Name ?? "Desconocido", // Nuevo
                order.Status,
                new List<OrderItemModel.ResponseOrderItemModel>() // Lista vacía
            );
        }

        public async Task<bool> CustomerExists(Guid customerId)
        {
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }
    }
}