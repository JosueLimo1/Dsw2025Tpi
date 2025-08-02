using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Dsw2025Tpi.Data;
using DSWIntegral.Models;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.Api.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly Dsw2025TpiContext _context;
        public ProductsController(Dsw2025TpiContext context) => _context = context;

        /// <summary>
        /// Lista todos los productos activos.
        /// </summary>
        /// <returns>Colección de productos activos.</returns>
        /// <response code="200">Retorna la lista de productos.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un producto por su identificador.
        /// </summary>
        /// <param name="id">ID del producto.</param>
        /// <returns>Objeto del producto.</returns>
        /// <response code="200">Producto encontrado.</response>
        /// <response code="404">Producto no hallado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("{id}")]
      //  [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound();
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo producto (solo Admin).
        /// </summary>
        /// <param name="product">Datos del producto a crear.</param>
        /// <returns>El producto creado.</returns>
        /// <response code="201">Producto creado exitosamente.</response>
        /// <response code="400">Solicitud inválida o SKU duplicado.</response>
        /// <response code="401">No autenticado.</response>
        /// <response code="403">Sin permiso (Admin requerido).</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost]
     //   [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                if (await _context.Products.AnyAsync(p => p.Sku == product.Sku))
                    return BadRequest(new ErrorResponse { Message = $"El SKU '{product.Sku}' ya está en uso.", Details = null });

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza todos los campos de un producto (solo Admin).
        /// </summary>
        /// <param name="id">ID del producto.</param>
        /// <param name="updatedProduct">Datos actualizados.</param>
        /// <response code="204">Actualización exitosa.</response>
        /// <response code="400">Solicitud inválida (ID mismatch o SKU duplicado).</response>
        /// <response code="404">Producto no encontrado.</response>
        /// <response code="401">No autenticado.</response>
        /// <response code="403">Sin permiso (Admin requerido).</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPut("{id}")]
     //   [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> UpdateProduct(Guid id, Product updatedProduct)
        {
            if (id != updatedProduct.Id)
                return BadRequest(new ErrorResponse { Message = "ID de ruta y body no coinciden.", Details = null });

            if (await _context.Products.AnyAsync(p => p.Sku == updatedProduct.Sku && p.Id != id))
                return BadRequest(new ErrorResponse { Message = $"El SKU '{updatedProduct.Sku}' ya está en uso.", Details = null });

            _context.Entry(updatedProduct).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Products.AnyAsync(p => p.Id == id))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva un producto (solo Admin).
        /// </summary>
        /// <param name="id">ID del producto.</param>
        /// <response code="204">Producto desactivado.</response>
        /// <response code="404">Producto no encontrado.</response>
        /// <response code="401">No autenticado.</response>
        /// <response code="403">Sin permiso (Admin requerido).</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPatch("{id}")]
      //  [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> PatchProduct(Guid id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound();

                product.IsActive = false;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Elimina físicamente un producto (solo Admin).
        /// </summary>
        /// <param name="id">ID del producto.</param>
        /// <response code="204">Producto eliminado.</response>
        /// <response code="404">Producto no encontrado.</response>
        /// <response code="401">No autenticado.</response>
        /// <response code="403">Sin permiso (Admin requerido).</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpDelete("{id}")]
      //  [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound();

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Error interno.", Details = ex.Message });
            }
        }
    } 
}