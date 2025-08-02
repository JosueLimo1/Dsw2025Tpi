using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Dsw2025Tpi.Data;
using DSWIntegral.Models;
using Dsw2025Tpi.Application.Exceptions;
namespace Dsw2025Tpi.Controllers
{
    /// <summary>
    /// Gestión de clientes. Solo administradores pueden usar estos endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
   // [Authorize(Policy = "AdminOnly")]
    public class CustomersController : ControllerBase
    {
        private readonly Dsw2025TpiContext _context;
        public CustomersController(Dsw2025TpiContext context) => _context = context;

        /// <summary>
        /// Lista todos los clientes.
        /// </summary>
        /// <returns>Lista de <see cref="Customer"/>.</returns>
        /// <response code="200">OK. Devuelve todos los clientes.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            try
            {
                var list = await _context.Customers.ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Obtiene un cliente por su ID.
        /// </summary>
        /// <param name="id">GUID del cliente.</param>
        /// <returns><see cref="Customer"/> solicitado.</returns>
        /// <response code="200">OK. Devuelve el cliente.</response>
        /// <response code="404">Not Found. Cliente no existe.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> GetCustomer(Guid id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound();
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Crea un nuevo cliente.
        /// </summary>
        /// <param name="customer">Datos del cliente a crear.</param>
        /// <returns>Cliente creado.</returns>
        /// <response code="201">Created. Cliente creado correctamente.</response>
        /// <response code="400">Bad Request. Email duplicado o datos inválidos.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
        {
            try
            {
                if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
                    return BadRequest(new ErrorResponse { Message = $"El email '{customer.Email}' ya existe." });

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (DbUpdateException dbex)
            {
                return BadRequest(new ErrorResponse {
                    Message = "Error al guardar el cliente.",
                    Details = dbex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Actualiza un cliente existente.
        /// </summary>
        /// <param name="id">GUID del cliente.</param>
        /// <param name="updated">Datos a actualizar.</param>
        /// <response code="204">No Content. Actualización exitosa.</response>
        /// <response code="400">Bad Request. ID no coincide o email duplicado.</response>
        /// <response code="404">Not Found. Cliente no existe.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] Customer updated)
        {
            try
            {
                if (id != updated.Id)
                    return BadRequest(new ErrorResponse { Message = "El ID de la URL no coincide con el ID del cliente." });

                if (await _context.Customers
                        .AnyAsync(c => c.Email == updated.Email && c.Id != id))
                    return BadRequest(new ErrorResponse { Message = $"El email '{updated.Email}' ya está en uso." });

                _context.Entry(updated).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Customers.AnyAsync(c => c.Id == id))
                    return NotFound();
                throw;
            }
            catch (DbUpdateException dbex)
            {
                return BadRequest(new ErrorResponse {
                    Message = "Error al actualizar el cliente.",
                    Details = dbex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Elimina un cliente.
        /// </summary>
        /// <param name="id">GUID del cliente a eliminar.</param>
        /// <response code="204">No Content. Eliminación exitosa.</response>
        /// <response code="404">Not Found. Cliente no existe.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                var cust = await _context.Customers.FindAsync(id);
                if (cust == null) return NotFound();

                _context.Customers.Remove(cust);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }
    }
}