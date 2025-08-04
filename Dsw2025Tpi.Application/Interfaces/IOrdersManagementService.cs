using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IOrdersManagementService
    {
        // Obtener una orden por ID
        Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id);

        // Obtener todas las órdenes con filtros opcionales
        Task<IEnumerable<OrderModel.ResponseOrderModel>> GetAllOrders(OrderFilterModel? filter = null);

        // Crear una nueva orden
        Task<OrderModel.ResponseOrderModel> CreateOrder(OrderModel.RequestOrderModel request);

        // Cambiar el estado de una orden (PUT /api/orders/{id}/status)
        Task<OrderModel.ResponseOrderModel?> UpdateOrderStatus(Guid id, OrderStatus newStatus);
    }
}

