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

        public async Task<IEnumerable<OrderModel.ResponseOrderModel>> GetAllOrders(OrderFilterModel? filter = null)
        {
            var query = _context.Orders.AsQueryable();

            if (filter is not null)
            {
                if (filter.CustomerId.HasValue)
                    query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

                if (filter.Status.HasValue)
                    query = query.Where(o => o.Status == filter.Status.Value);
            }

            var orders = await query.ToListAsync();

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

        public async Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return null;

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

        public async Task<OrderModel.ResponseOrderModel?> UpdateOrderStatus(Guid id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null) return null;

            order.ChangeStatus(newStatus);
            await _context.SaveChangesAsync();

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

        public async Task<bool> CustomerExists(Guid customerId)
        {
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }
    }
}