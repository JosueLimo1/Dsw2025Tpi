using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // Ruta base: /api/orders
[Authorize] // Todos los endpoints requieren token, salvo los que tengan [AllowAnonymous]
public class OrdersController : ControllerBase
{
    private readonly IOrdersManagementService _ordersService;

    // Se inyecta el servicio que contiene la lógica para manejar órdenes
    public OrdersController(IOrdersManagementService ordersService)
    {
        _ordersService = ordersService;
    }

    // POST: /api/orders
    // Permite a un cliente (rol User) registrar una nueva orden
    [HttpPost]
    [Authorize(Roles = "User")] // Solo clientes pueden crear órdenes
    [ProducesResponseType(StatusCodes.Status201Created)] // Devuelve 201 si se crea correctamente
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Devuelve 400 si hay errores
    public async Task<IActionResult> Create([FromBody] OrderModel.RequestOrderModel request)
    {
        // Se obtiene el ID del usuario autenticado desde el token JWT
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Se valida que el ID del cliente coincida con el ID del usuario autenticado
        if (string.IsNullOrEmpty(userId) || request.CustomerId.ToString() != userId)
            return BadRequest("El ID del cliente no coincide con el usuario autenticado.");

        try
        {
            // Se delega la creación al servicio
            var createdOrder = await _ordersService.CreateOrder(request);

            // Se devuelve 201 con la ruta para consultar la orden
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message); // Error por datos inválidos
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // Error por stock insuficiente u otra lógica
        }
    }

    // GET: /api/orders
    // Permite obtener todas las órdenes con filtros opcionales
    // Los clientes solo pueden ver sus órdenes, los admins pueden ver todas
    [HttpGet]
    [Authorize(Roles = "User,Admin")] // Tanto Admin como User pueden acceder
    public async Task<IActionResult> GetAll([FromQuery] OrderFilterModel? filter)
    {
        // Si es cliente, debe consultar solo sus órdenes
        if (User.IsInRole("User"))
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Validar que se esté usando su propio customerId
            if (filter == null || string.IsNullOrEmpty(filter.CustomerId.ToString()))
                return BadRequest("Falta el parámetro customerId para el cliente.");

            if (filter.CustomerId.ToString() != userId)
                return Forbid("No podés ver órdenes de otro cliente.");
        }

        // Se obtienen las órdenes desde el servicio
        var orders = await _ordersService.GetAllOrders(filter);
        return Ok(orders); // 200 OK con la lista de órdenes
    }

    // GET: /api/orders/{id}
    // Devuelve los detalles de una orden específica
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")] // Solo los administradores pueden consultar cualquier orden
    [ProducesResponseType(StatusCodes.Status200OK)] // Devuelve 200 si existe
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Devuelve 404 si no existe
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _ordersService.GetOrderById(id);

        // Si no existe, devolver 404
        return order == null ? NotFound() : Ok(order);
    }

    // PUT: /api/orders/{id}/status
    // Permite a los administradores cambiar el estado de una orden
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")] // Solo admins pueden cambiar el estado
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        try
        {
            var updated = await _ordersService.UpdateOrderStatus(id, dto.NewStatus);

            // Si no se encuentra la orden, devolver 404
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message); // Error por transición de estado inválida
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // Otros errores de negocio
        }
    }

    // Record simple para actualizar el estado de una orden
    public record UpdateStatusDto(OrderStatus NewStatus);
}
