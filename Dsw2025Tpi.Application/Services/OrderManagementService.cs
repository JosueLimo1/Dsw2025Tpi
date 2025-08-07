using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Validation;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Application.Services
{
    // Implementación del servicio de gestión de órdenes que cumple con la interfaz IOrdersManagementService
    public class OrdersManagementService : IOrdersManagementService
    {
        // Campo privado que representa el contexto de base de datos (Entity Framework)
        //El contexto de base de datos (_context) es como una llave especial que te da ASP.NET
        //para que puedas entrar a la base de datos y trabajar con los datos.
        private readonly Dsw2025TpiContext _context;


        // Constructor que recibe el contexto inyectado por dependencia
        public OrdersManagementService(Dsw2025TpiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea una nueva orden con sus ítems.
        /// Se valida que el cliente exista, que los productos existan y tengan stock suficiente.
        /// Al crear la orden se descuenta automáticamente el stock de los productos involucrados.
        /// </summary>
        public async Task<OrderModel.ResponseOrderModel> CreateOrder(OrderModel.RequestOrderModel request)
        {
            // 🔸 VALIDACIÓN: Validamos la estructura general del request
            OrderValidator.Validate(request);

            // Se busca el cliente en la base de datos
            var customer = await _context.Customers.FindAsync(request.CustomerId);

            // 🔸 VALIDACIÓN: Usamos tu clase para validar si el cliente existe y sus datos básicos
            CustomerValidator.Validate(customer);

            // Se crea una nueva instancia de orden (Order) con los datos del request
            var order = new Order(
                DateTime.UtcNow, // Fecha actual UTC
                request.ShippingAddress,
                request.BillingAddress,
                null, // notas opcionales, se pasa null por ahora
                request.CustomerId)
            {
                Id = Guid.NewGuid() // Se asigna un nuevo identificador único (GUID)
            };

            // Se recorren los ítems de la orden para agregarlos a la orden
            foreach (var item in request.OrderItems)
            {
                // 🔸 VALIDACIÓN: Validamos los campos del ítem (productoId y cantidad)
                OrderItemValidator.Validate(item);

                // Se busca el producto correspondiente en la base
                var product = await _context.Products.FindAsync(item.ProductId);

                // 🔸 VALIDACIÓN: Validamos si el producto existe, está activo, y sus datos
                ProductValidator.Validate(product!); // usamos ! porque ya validamos en la clase

                // 🔸 VALIDACIÓN: Verificamos que haya stock suficiente
                ProductValidator.EnsureStock(product!, item.Quantity);

                // Se agrega el ítem a la orden (también valida stock y descuenta)
                order.AddItem(product, item.Quantity);
            }

            // Se agrega la orden al contexto para ser persistida
            _context.Orders.Add(order);

            // Se guardan los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Se devuelve el modelo de respuesta con los datos de la orden recién creada
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
        /// Obtiene todas las órdenes, opcionalmente aplicando filtros por cliente o estado.
        /// </summary>
        public async Task<IEnumerable<OrderModel.ResponseOrderModel>> GetAllOrders(OrderFilterModel? filter = null)
        {
            // Se obtiene la colección de órdenes como queryable para aplicar filtros dinámicamente
            //Cuando algo es queryable (como IQueryable en C#), significa que todavía no se ha ejecutado la consulta,
            //pero podés seguir agregándole filtros o condiciones antes de que se ejecute.
            var query = _context.Orders.AsQueryable();

            // Si se proporciona un filtro, se aplican las condiciones
            if (filter is not null)
            {
                // Filtro por ID de cliente
                if (filter.CustomerId.HasValue)
                    query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

                // Filtro por estado de la orden
                if (filter.Status.HasValue)
                    query = query.Where(o => o.Status == filter.Status.Value);
            }

            // Se ejecuta la consulta y se obtiene la lista de órdenes
            var orders = await query.ToListAsync();

            // Se mapean las órdenes a su modelo de respuesta correspondiente
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
        /// Busca una orden por su ID, incluyendo los ítems y productos relacionados.
        /// </summary>
        public async Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id)
        {
            // Se busca la orden incluyendo los ítems y productos (join con include)
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            // Si no se encuentra, se devuelve null
            if (order is null) return null;

            // Se devuelve la orden en formato de respuesta
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
        /// Actualiza el estado de una orden existente.
        /// </summary>
        public async Task<OrderModel.ResponseOrderModel?> UpdateOrderStatus(Guid id, OrderStatus newStatus)
        {
            // Se busca la orden por su ID
            var order = await _context.Orders.FindAsync(id);

            // Si no existe, se devuelve null
            if (order is null) return null;

            // Se cambia el estado de la orden (internamente puede validar reglas de negocio)
            order.ChangeStatus(newStatus);

            // Se guardan los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Se devuelve la orden actualizada
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
        /// Verifica si existe un cliente con el ID proporcionado.
        /// </summary>
        public async Task<bool> CustomerExists(Guid customerId)
        {
            // Devuelve true si encuentra un cliente con ese ID, false en caso contrario
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }
    }
}
