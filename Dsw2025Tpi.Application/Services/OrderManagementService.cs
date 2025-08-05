using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Application.Services;

// Implementación del servicio de gestión de órdenes
public class OrdersManagementService : IOrdersManagementService
{
    // Contexto de base de datos inyectado
    private readonly Dsw2025TpiContext _context;

    // Constructor que recibe el contexto por inyección de dependencias
    public OrdersManagementService(Dsw2025TpiContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea una nueva orden con sus ítems, validando el stock de cada producto.
    /// Si hay stock suficiente, se descuenta automáticamente.
    /// </summary>
    public async Task<OrderModel.ResponseOrderModel> CreateOrder(OrderModel.RequestOrderModel request)
    {
        // Verifica que el cliente exista en la base
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer is null)
            throw new ArgumentException("Cliente no encontrado");

        // Crea una nueva orden con los datos proporcionados (sin ítems aún)
        var order = new Order(
           DateTime.UtcNow, // Fecha actual del sistema
           request.ShippingAddress,
           request.BillingAddress,
           null, // El campo Notes fue eliminado del request
           request.CustomerId)
        {
            Id = Guid.NewGuid() // Genera un nuevo ID para la orden
        };

        // Recorre todos los ítems enviados en la orden
        foreach (var item in request.OrderItems)
        {
            // Verifica que el producto exista
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product is null)
                throw new ArgumentException($"Producto con ID {item.ProductId} no encontrado");

            // Agrega el ítem a la orden y valida stock y estado del producto
            order.AddItem(product, item.Quantity);
        }

        // Agrega la orden a la base de datos y guarda los cambios
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Devuelve un DTO con los datos de la orden creada
        return new OrderModel.ResponseOrderModel(
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.CustomerId,
            order.Status
        );
    }

    /// <summary>
    /// Devuelve todas las órdenes registradas, con filtros opcionales por estado y cliente.
    /// </summary>
    public async Task<IEnumerable<OrderModel.ResponseOrderModel>> GetAllOrders(OrderFilterModel? filter = null)
    {
        // Consulta base sobre la tabla Orders
        var query = _context.Orders.AsQueryable();

        // Aplica filtro por cliente si se especificó
        if (filter is not null)
        {
            if (filter.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

            // Aplica filtro por estado si se especificó
            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);
        }

        // Ejecuta la consulta
        var orders = await query.ToListAsync();

        // Convierte cada orden a su modelo de respuesta
        return orders.Select(o => new OrderModel.ResponseOrderModel(
            o.Id,
            o.Date,
            o.ShippingAddress,
            o.BillingAddress,
            o.Notes,
            o.CustomerId,
            o.Status
        ));
    }

    /// <summary>
    /// Devuelve una orden específica, incluyendo sus ítems y productos asociados.
    /// </summary>
    public async Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id)
    {
        // Incluye ítems y productos relacionados a la orden
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        // Si no existe la orden, devuelve null
        if (order is null) return null;

        // Devuelve el modelo de respuesta
        return new OrderModel.ResponseOrderModel(
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.CustomerId,
            order.Status
        );
    }

    /// <summary>
    /// Cambia el estado de una orden existente.
    /// </summary>
    public async Task<OrderModel.ResponseOrderModel?> UpdateOrderStatus(Guid id, OrderStatus newStatus)
    {
        // Busca la orden por ID
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return null;

        // Cambia su estado
        order.ChangeStatus(newStatus);

        // Guarda cambios
        await _context.SaveChangesAsync();

        // Devuelve la orden actualizada
        return new OrderModel.ResponseOrderModel(
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.CustomerId,
            order.Status
        );
    }

    // Verifica si un cliente existe (método auxiliar)
    public async Task<bool> CustomerExists(Guid customerId)
    {
        return await _context.Customers.AnyAsync(c => c.Id == customerId);
    }
}
