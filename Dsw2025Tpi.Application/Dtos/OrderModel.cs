using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Dtos
{
    public record OrderModel
    {
        public record RequestOrderModel(Guid CustomerId, string? ShippingAddress, string? BillingAddress, List<OrderItemModel.RequestOrderItemModel> OrderItems);
        //public record OrderItemRequest(Guid ProductId, int Quantity);
        public record ResponseOrderModel(Guid Id, DateTime Date, string? ShippingAddress, string? BillingAddress, string? Notes, Guid CustomerId, OrderStatus Status);
    
    }
}
