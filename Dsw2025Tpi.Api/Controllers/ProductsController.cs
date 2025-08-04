using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación por defecto para todos los endpoints
public class ProductsController : ControllerBase
{
    private readonly IProductsManagementService _productsService;

    public ProductsController(IProductsManagementService productsService)
    {
        _productsService = productsService;
    }

    // Crea un nuevo producto.
    // Solo accesible para usuarios con rol Admin.
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProductModel.RequestProductModel model)
    {
        try
        {
            var created = await _productsService.AddProduct(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

    // Devuelve todos los productos activos.
    // Este endpoint es público (sin autenticación).
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productsService.GetAllProducts();
        var active = products?.Where(p => p.IsActive).ToList();

        if (active == null || !active.Any())
            return NoContent();

        return Ok(active);
    }

    // Devuelve un producto específico por ID.
    // Accesible para usuarios con rol Admin o User.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productsService.GetProductById(id);
        if (product == null || !product.IsActive)
            return NotFound();

        return Ok(product);
    }

    // Actualiza los datos de un producto existente.
    // Solo accesible para usuarios con rol Admin.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductModel.RequestProductModel model)
    {
        try
        {
            var updated = await _productsService.UpdateProduct(id, model);
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

    // Inhabilita un producto (cambia IsActive a false).
    // Solo accesible para usuarios con rol Admin.
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disable(Guid id)
    {
        var success = await _productsService.DisableProduct(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
