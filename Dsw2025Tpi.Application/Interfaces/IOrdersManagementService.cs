using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Interfaces;

// Interfaz que define las operaciones disponibles para el manejo de órdenes
public interface IOrdersManagementService
{
    // Crea una nueva orden a partir del modelo de request
    Task<OrderModel.ResponseOrderModel> CreateOrder(OrderModel.RequestOrderModel request);

    // Obtiene todas las órdenes con filtro opcional
    Task<IEnumerable<OrderModel.ResponseOrderModel>> GetAllOrders(OrderFilterModel? filter = null);

    // Devuelve una orden específica por su ID
    Task<OrderModel.ResponseOrderModel?> GetOrderById(Guid id);

    // Cambia el estado de una orden
    Task<OrderModel.ResponseOrderModel?> UpdateOrderStatus(Guid id, OrderStatus newStatus);

    // Verifica si un cliente existe (método auxiliar)
    Task<bool> CustomerExists(Guid customerId);
}



