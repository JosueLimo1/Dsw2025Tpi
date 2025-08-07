using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Dsw2025Tpi.Api.Controllers
{
    // Marca la clase como controlador de API REST
    [ApiController]

    // Define la ruta base para todos los endpoints: /api/orders
    [Route("api/[controller]")]

    // Requiere autenticación JWT para todos los endpoints de este controlador (salvo que se indique lo contrario)
    [Authorize]
    public class OrdersController : ControllerBase
    {
        // Servicio inyectado que maneja la lógica relacionada con órdenes
        private readonly IOrdersManagementService _ordersService;

        // Constructor con inyección de dependencias
        public OrdersController(IOrdersManagementService ordersService)
        {
            _ordersService = ordersService;
        }

        // ================================
        // POST: /api/orders
        // Crea una nueva orden
        // Solo pueden acceder los usuarios con rol "User"
        // ================================
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created)]       // 201 si se crea correctamente
        [ProducesResponseType(StatusCodes.Status400BadRequest)]    // 400 si falla validación
        public async Task<IActionResult> Create([FromBody] OrderModel.RequestOrderModel request)
        {
            // Se obtiene el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Se verifica que el ID del cliente enviado coincida con el ID del usuario autenticado
            // Esta es una validación técnica, no de dominio → se maneja directamente acá
            if (string.IsNullOrEmpty(userId) || request.CustomerId.ToString() != userId)
                return BadRequest("El ID del cliente no coincide con el usuario autenticado.");

            // Se delega la lógica al servicio, que lanzará excepciones personalizadas si algo falla
            // Llama al método CreateOrder del servicio
            var createdOrder = await _ordersService.CreateOrder(request);

            // Devuelve 201 Created con la URL para consultar la orden recién creada
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }

        // ================================
        // GET: /api/orders
        // Devuelve todas las órdenes del sistema (Admin) o del usuario actual (User)
        // ================================
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] OrderFilterModel? filter)
        {
            // Si el usuario es un cliente (rol User), debe consultar solo sus órdenes
            if (User.IsInRole("User"))
            {
                // Se extrae el ID del usuario autenticado
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Validación: si no se proporciona el customerId en el filtro, se devuelve 400
                if (filter == null || filter.CustomerId == Guid.Empty)
                    return BadRequest("Falta el parámetro customerId para el cliente.");

                // Si el cliente intenta acceder a órdenes de otro, se bloquea
                if (filter.CustomerId.ToString() != userId)
                    return Forbid("No podés ver órdenes de otro cliente.");
            }

            // Se llama al servicio para obtener las órdenes, según filtros y rol
            var orders = await _ordersService.GetAllOrders(filter);

            // Devuelve 200 OK con la lista
            return Ok(orders);
        }

        // ================================
        // GET: /api/orders/{id}
        // Devuelve los detalles de una orden específica
        // Solo los administradores pueden acceder a este endpoint
        // ================================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]          // 200 si se encuentra la orden
        [ProducesResponseType(StatusCodes.Status404NotFound)]    // 404 si no se encuentra
        public async Task<IActionResult> GetById(Guid id)
        {
            // Se busca la orden por su ID
            var order = await _ordersService.GetOrderById(id);

            // Si no existe, se devuelve 404 Not Found
            return order == null ? NotFound() : Ok(order);
        }

        // ================================
        // PUT: /api/orders/{id}/status
        // Actualiza el estado de una orden (solo Admin)
        // ================================
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]           // 200 si se actualiza
        [ProducesResponseType(StatusCodes.Status400BadRequest)]   // 400 si falla validación de estado
        [ProducesResponseType(StatusCodes.Status404NotFound)]     // 404 si no se encuentra la orden
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            // Se delega la lógica de actualización al servicio
            var updated = await _ordersService.UpdateOrderStatus(id, dto.NewStatus);

            // Si no se encontró la orden, devuelve 404
            return updated == null ? NotFound() : Ok(updated);
        }
        
    }
}

