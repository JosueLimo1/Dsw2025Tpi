using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Application.Services;

public class OrdersManagementService : IOrdersManagementService
{
    private readonly Dsw2025TpiContext _context;

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
        // 1. Validar que el cliente exista
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer is null)
            throw new ArgumentException("Cliente no encontrado");

        // 2. Crear la orden base con sus datos (sin ítems aún)
        var order = new Order(
            request.Date,
            request.ShippingAddress,
            request.BillingAddress,
            request.Notes,
            request.CustomerId)
        {
            Id = Guid.NewGuid() // el ID lo generamos nosotros (como pide el TPI)
        };

        // 3. Procesar cada ítem incluido en la orden
        foreach (var item in request.Items)
        {
            // 3.1 Validar que el producto exista
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product is null)
                throw new ArgumentException($"Producto con ID {item.ProductId} no encontrado");

            // 3.2 Agregar el ítem a la orden (valida stock, cantidad > 0 y si el producto está activo)
            // También descuenta el stock del producto si todo es válido
            order.AddItem(product, item.Quantity);
        }

        // 4. Guardar la orden y sus ítems en la base de datos
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 5. Retornar un DTO con la información básica de la orden creada
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
        // 1. Construir el query base sobre la tabla Orders
        var query = _context.Orders.AsQueryable();

        // 2. Aplicar filtros si fueron enviados
        if (filter is not null)
        {
            if (filter.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);
        }

        // 3. Ejecutar la consulta a la base de datos
        var orders = await query.ToListAsync();

        // 4. Mapear cada orden a su DTO de respuesta
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
        // 1. Trae la orden e incluye los ítems y los productos de cada ítem
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        // 2. Si no existe, retorna null
        if (order is null) return null;

        // 3. Mapear a DTO de respuesta
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
    /// Cambia el estado de una orden existente (ej: de Pendiente a Enviado).
    /// </summary>
    public async Task<OrderModel.ResponseOrderModel?> UpdateOrderStatus(Guid id, OrderStatus newStatus)
    {
        // 1. Buscar la orden por ID
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return null;

        // 2. Cambiar el estado de la orden
        order.ChangeStatus(newStatus);

        // 3. Guardar cambios
        await _context.SaveChangesAsync();

        // 4. Retornar la orden actualizada
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
}

