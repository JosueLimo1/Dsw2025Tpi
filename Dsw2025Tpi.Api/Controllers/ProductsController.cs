using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

// Controlador que maneja las operaciones sobre productos
[ApiController] // Indica que esta clase es un controlador de API REST
[Route("api/[controller]")] // Define la ruta base como /api/products
[Authorize] // Requiere autenticación JWT para acceder (por defecto para todos los endpoints)
public class ProductsController : ControllerBase // Hereda de ControllerBase, clase base para APIs
{
    private readonly IProductsManagementService _productsService; // Servicio inyectado para manejar productos

    // Constructor que recibe el servicio por inyección de dependencias
    public ProductsController(IProductsManagementService productsService)
    {
        _productsService = productsService;
    }

    // POST /api/products
    // Crea un nuevo producto - solo para admins
    [HttpPost]
    [Authorize(Roles = "Admin")] // Solo administradores pueden acceder a este endpoint
    public async Task<IActionResult> Create([FromBody] ProductModel.RequestProductModel model)
    {
        try
        {
            // Llama al servicio para agregar el producto
            var created = await _productsService.AddProduct(model);

            // Devuelve 201 Created con la URL de acceso al recurso
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            // Devuelve 400 Bad Request si hay errores de validación
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Devuelve 400 Bad Request si hay errores de lógica (negocio)
            return BadRequest(ex.Message);
        }
    }

    // GET /api/products
    // Devuelve todos los productos activos
    [HttpGet]
    [AllowAnonymous] // Este endpoint es público, no requiere autenticación
    public async Task<IActionResult> GetAll()
    {
        // Obtiene todos los productos desde el servicio
        var products = await _productsService.GetAllProducts();

        // Filtra solo los que están activos
        var active = products?.Where(p => p.IsActive).ToList();

        // Si no hay productos activos, devuelve 204 No Content
        if (active == null || !active.Any())
            return NoContent();

        // Devuelve 200 OK con la lista de productos activos
        return Ok(active);
    }

    // GET /api/products/{id}
    // Devuelve un producto específico por su ID
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,User")] // Admins y clientes pueden acceder
    public async Task<IActionResult> GetById(Guid id)
    {
        // Busca el producto por ID
        var product = await _productsService.GetProductById(id);

        // Si no existe o está inactivo, devuelve 404
        if (product == null || !product.IsActive)
            return NotFound();

        // Devuelve 200 OK con el producto
        return Ok(product);
    }

    // PUT /api/products/{id}
    // Actualiza los datos de un producto
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Solo administradores pueden modificar productos
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductModel.RequestProductModel model)
    {
        try
        {
            // Intenta actualizar el producto
            var updated = await _productsService.UpdateProduct(id, model);

            // Si no se encontró el producto, devuelve 404
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            // Errores de validación
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Errores de negocio
            return BadRequest(ex.Message);
        }
    }

    // PATCH /api/products/{id}
    // Inhabilita un producto sin eliminarlo (soft delete)
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")] // Solo administradores pueden deshabilitar productos
    public async Task<IActionResult> Disable(Guid id)
    {
        // Llama al servicio para desactivar el producto
        var success = await _productsService.DisableProduct(id);

        // Devuelve 204 No Content si se logró, 404 si no existe
        return success ? NoContent() : NotFound();
    }
}

