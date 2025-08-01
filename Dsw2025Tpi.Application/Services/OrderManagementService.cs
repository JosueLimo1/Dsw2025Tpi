using Azure.Core;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Validation;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService : IOrdersManagementService
    {
        // Campo privado solo de lectura que almacena la referencia al repositorio inyectado
        private readonly IRepository _repository;

        // Constructor que recibe el repositorio por inyección de dependencias
        public OrdersManagementService(IRepository repository)
        {
            _repository = repository;
        }

        // Método asincrónico que devuelve una orden por su ID
        public async Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id)
        {
            // Busca la orden en el repositorio incluyendo las propiedades de navegación: OrderItems y Product
            var order = await _repository.GetById<Order>(id, nameof(Order.OrderItems), "OrderItems.Product");

            // Si no existe la orden, lanza una excepción
            if (order == null) throw new InvalidOperationException($"Orden no encontrada");

            // Si existe, la mapea a un modelo de respuesta y la retorna
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

        // Método asincrónico que devuelve todas las órdenes en el sistema
        public async Task<IEnumerable<OrderModel.ResponseOrderModel>?> GetAllOrders()
        {
            // Obtiene todas las órdenes del repositorio
            var orders = await _repository.GetAll<Order>();

            // Mapea cada orden al modelo de respuesta y devuelve el listado
            return orders?.Select(o => new OrderModel.ResponseOrderModel(
                o.Id,
                o.Date,
                o.ShippingAddress,
                o.BillingAddress,
                o.Notes,
                o.CustomerId,
                o.Status
            ));
        }

        // Método asincrónico para agregar una nueva orden al sistema
        public async Task<OrderModel.ResponseOrderModel> AddOrder(OrderModel.RequestOrderModel request)
        {
            // Valida la orden usando un validador estático
            OrderValidator.Validate(request);

            // Verifica que la orden contenga al menos un ítem
            if (request.Items == null || !request.Items.Any())
                throw new ArgumentException("La orden debe tener al menos un item.");

            // Crea una instancia de la entidad Order sin los ítems
            var order = new Order(
                request.Date,
                request.ShippingAddress,
                request.BillingAddress,
                request.Notes,
                request.CustomerId
            );

            // Guarda la orden para obtener su ID (si es necesario)
            await _repository.Add(order);

            // Lista para almacenar los ítems que se van a asociar a la orden
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            // Itera por cada ítem enviado en el request
            foreach (var item in request.Items)
            {
                // Busca el producto correspondiente en la base de datos
                var product = await _repository.GetById<Product>(item.ProductId)
                    ?? throw new InvalidOperationException($"Producto no encontrado: {item.ProductId}");

                // Verifica si hay suficiente stock
                if (product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Stock insuficiente para el producto: {product.Name}");

                // Descuenta del stock la cantidad solicitada
                product.StockQuantity -= item.Quantity;

                // Actualiza el producto en la base de datos
                await _repository.Update(product);

                // Crea un nuevo OrderItem con la información correspondiente
                var orderItem = new OrderItem(
                    item.Quantity,
                    product.CurrentUnitPrice,
                    order.Id,       // ID de la orden ya guardada
                    product.Id      // ID del producto
                );

                // Agrega el ítem a la lista
                orderItems.Add(orderItem);

                // Suma al total el monto correspondiente al producto
                totalAmount += product.CurrentUnitPrice * item.Quantity;
            }

            // Asigna los ítems a la orden
            order.OrderItems = orderItems;

            // Actualiza la orden en la base de datos con los ítems agregados
            await _repository.Update(order);

            // Crea una lista de modelos de respuesta para los ítems (opcional si se quiere devolver en respuesta completa)
            var responseItems = orderItems.Select(oi => new OrderItemModel.ResponseOrderItemModel(
                oi.Id,
                oi.Quantity,
                oi.UnitPrice,
                oi.OrderId,
                oi.ProductId
            )).ToList();

            // Retorna la orden completa como modelo de respuesta
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

        // Método asincrónico para actualizar el estado de una orden existente
        public async Task<OrderModel.ResponseOrderModel> PutOrder(Guid id, OrderModel.RequestOrderModel request)
        {
            // Valida el contenido del request
            OrderValidator.Validate(request);

            // Verifica que el estado enviado sea válido dentro del enum OrderStatus
            if (!Enum.IsDefined(typeof(OrderStatus), request.Status))
            {
                throw new ArgumentOutOfRangeException("El estado ingresado no es válido.");
            }

            // Busca la orden por ID
            var exist = await _repository.GetById<Order>(id);

            // Si no existe, lanza excepción
            if (exist == null)
                throw new KeyNotFoundException($"No se encontró la orden con ID: {id}");

            // Actualiza el estado de la orden
            exist.Status = request.Status;

            // Guarda los cambios
            await _repository.Update(exist);

            // Devuelve la orden actualizada como modelo de respuesta
            return new OrderModel.ResponseOrderModel(
                exist.Id,
                exist.Date,
                exist.ShippingAddress,
                exist.BillingAddress,
                exist.Notes,
                exist.CustomerId,
                exist.Status
            );
        }
    }
}
