using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersManagementService _ordersService;

        public OrdersController(IOrdersManagementService ordersService)
        {
            _ordersService = ordersService;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] OrderModel.RequestOrderModel request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || request.CustomerId.ToString() != userId)
                return BadRequest("El ID del cliente no coincide con el usuario autenticado.");

            var createdOrder = await _ordersService.CreateOrder(request);

            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] OrderFilterModel? filter)
        {
            if (User.IsInRole("User"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (filter == null || filter.CustomerId == Guid.Empty)
                    return BadRequest("Falta el parámetro customerId para el cliente.");

                if (filter.CustomerId.ToString() != userId)
                    return Forbid("No podés ver órdenes de otro cliente.");
            }

            var orders = await _ordersService.GetAllOrders(filter);

            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _ordersService.GetOrderById(id);

            return order == null ? NotFound() : Ok(order);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var updated = await _ordersService.UpdateOrderStatus(id, dto.NewStatus);

            return updated == null ? NotFound() : Ok(updated);
        }
    }
}