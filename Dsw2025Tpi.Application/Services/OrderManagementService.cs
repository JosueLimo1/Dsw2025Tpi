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
        private readonly IRepository _repository;

        public OrdersManagementService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id)
        {
            var order = await _repository.GetById<Order>(id, nameof(Order.OrderItems), "OrderItems.Product");
            if (order == null) throw new InvalidOperationException($"Orden no encontrada");
            return order != null ?
                new OrderModel.ResponseOrderModel(order.Id, order.Date, order.ShippingAddress, order.BillingAddress, order.Notes, order.CustomerId, order.Status) :
                null;
        }

        // Solucionar aqui y ver si usamos filtro o un GetAll
        public async Task<IEnumerable<OrderModel.ResponseOrderModel>?> GetAllOrders()
        {
            return (await _repository
                .GetAll<Order>())?
                .Select(o => new OrderModel.ResponseOrderModel(o.Id, o.Date, o.ShippingAddress, o.BillingAddress, o.Notes,
                o.CustomerId, o.Status));
        }

        // Hay que ver como podemos validar la fecha o si hace falta, ademas de como verificar que haya suficiente stock al momento de crear la orden y restar la cantidad de los  producto solicitado al stock de los prodcutos

        public async Task<OrderModel.ResponseOrderModel> AddOrder(OrderModel.RequestOrderModel request)
        {
            // Validación de la orden
            OrderValidator.Validate(request);

            if (request.Items == null || !request.Items.Any())
                throw new ArgumentException("La orden debe tener al menos un item.");

            // Crear la orden primero (sin items)
            var order = new Order(
                request.Date,
                request.ShippingAddress,
                request.BillingAddress,
                request.Notes,
                request.CustomerId
            );

            // Guardar la orden para obtener el Id (si es necesario para los OrderItem)
            await _repository.Add(order);

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                // Validación de existencia del producto
                var product = await _repository.GetById<Product>(item.ProductId)
                    ?? throw new InvalidOperationException($"Producto no encontrado: {item.ProductId}");

                // Validación de stock
                if (product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Stock insuficiente para el producto: {product.Name}");

                // Descuento de stock y actualización
                product.StockQuantity -= item.Quantity;
                await _repository.Update(product);

                // Creación del OrderItem usando el constructor correcto
                var orderItem = new OrderItem(
                    item.Quantity,
                    product.CurrentUnitPrice,
                    order.Id,
                    product.Id
                );
                orderItems.Add(orderItem);
                totalAmount += product.CurrentUnitPrice * item.Quantity;
            }

            // Asignar los items a la orden y actualizar la orden
            order.OrderItems = orderItems;
            await _repository.Update(order);

            // Mapeo de los items para la respuesta
            var responseItems = orderItems.Select(oi => new OrderItemModel.ResponseOrderItemModel(
                oi.Id,
                oi.Quantity,
                oi.UnitPrice,
                oi.OrderId,
                oi.ProductId
            )).ToList();

            // Retorno del modelo de respuesta
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

        public async Task<OrderModel.ResponseOrderModel> PutOrder(Guid id, OrderModel.RequestOrderModel request)
        {
            OrderValidator.Validate(request);

            if (!Enum.IsDefined(typeof(OrderStatus), request.Status))
            {
                throw new ArgumentOutOfRangeException("El estado ingresado no es válido.");
            }

            var exist = await _repository.GetById<Order>(id);
            if (exist == null)
                throw new KeyNotFoundException($"No se encontró la orden con ID: {id}");
            exist.Status = request.Status;

            await _repository.Update(exist);

            return new OrderModel.ResponseOrderModel
           (
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
