using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;


// Controlador que maneja las operaciones sobre productos
[ApiController]
[Route("api/[controller]")] // Ruta base: /api/products
[Authorize] // Todos los endpoints requieren autenticación (salvo los que indiquen lo contrario)
public class ProductsController : ControllerBase
{
    private readonly IProductsManagementService _productsService;

    public ProductsController(IProductsManagementService productsService)
    {
        _productsService = productsService;
    }

    // POST /api/products
    // Crea un nuevo producto - solo admins
    [HttpPost]
    [Authorize(Roles = "Admin")] // Solo administradores pueden crear productos
    public async Task<IActionResult> Create([FromBody] ProductModel.RequestProductModel model)
    {
        try
        {
            var created = await _productsService.AddProduct(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message); // Errores de validación
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // Errores de lógica de negocio
        }
    }

    // GET /api/products
    // Lista todos los productos activos - público
    [HttpGet]
    [AllowAnonymous] // Cualquier visitante puede consultar productos
    public async Task<IActionResult> GetAll()
    {
        var products = await _productsService.GetAllProducts();
        var active = products?.Where(p => p.IsActive).ToList();

        if (active == null || !active.Any())
            return NoContent(); // Si no hay productos activos

        return Ok(active); // Devuelve 200 OK con la lista
    }

    // GET /api/products/{id}
    // Obtiene un producto específico - clientes y admins
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productsService.GetProductById(id);

        if (product == null || !product.IsActive)
            return NotFound(); // 404 si no se encuentra o está inactivo

        return Ok(product); // Devuelve el producto
    }

    // PUT /api/products/{id}
    // Actualiza los datos de un producto - solo admins
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductModel.RequestProductModel model)
    {
        try
        {
            var updated = await _productsService.UpdateProduct(id, model);
            return updated == null ? NotFound() : Ok(updated);
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

    // PATCH /api/products/{id}
    // Inhabilita un producto (lo marca como no disponible)
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disable(Guid id)
    {
        var success = await _productsService.DisableProduct(id);
        return success ? NoContent() : NotFound();
    }
}
