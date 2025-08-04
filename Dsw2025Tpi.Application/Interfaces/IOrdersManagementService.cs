using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Interfaces;

public interface IOrdersManagementService
{
    Task<OrderModel.ResponseOrderModel> CreateOrder(OrderModel.RequestOrderModel request);
    Task<IEnumerable<OrderModel.ResponseOrderModel>> GetAllOrders(OrderFilterModel? filter = null);
    Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id);
    Task<OrderModel.ResponseOrderModel?> UpdateOrderStatus(Guid id, OrderStatus newStatus);
    Task<bool> CustomerExists(Guid customerId);//Método auxiliar agregado para validar existencia de cliente
}


