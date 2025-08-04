using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación por defecto para todos los endpoints
public class OrdersController : ControllerBase
{
    private readonly IOrdersManagementService _ordersService;

    public OrdersController(IOrdersManagementService ordersService)
    {
        _ordersService = ordersService;
    }

    // Crea una nueva orden de compra.
    // Solo accesible para usuarios con rol Admin.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] OrderModel.RequestOrderModel request)
    {
        try
        {
            var createdOrder = await _ordersService.CreateOrder(request);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Devuelve todas las órdenes existentes, con filtros opcionales.
    // Este endpoint es público (sin autenticación).
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] OrderFilterModel? filter)
    {
        try
        {
            if (filter?.CustomerId is Guid customerId)
            {
                var customerExists = await _ordersService.CustomerExists(customerId);
                if (!customerExists)
                    return BadRequest("El cliente indicado no existe.");
            }

            var orders = await _ordersService.GetAllOrders(filter);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    // Devuelve una orden específica por su ID.
    // Solo accesible para usuarios con rol Admin.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _ordersService.GetOrderById(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    // Cambia el estado de una orden (por ejemplo, de Pendiente a Enviado).
    // Solo accesible para usuarios con rol Admin.
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        try
        {
            var updated = await _ordersService.UpdateOrderStatus(id, dto.NewStatus);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DTO auxiliar para actualizar el estado de una orden
    public record UpdateStatusDto(OrderStatus NewStatus);
}
