using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    // Controlador que maneja las operaciones sobre productos
    [ApiController] // Marca esta clase como controlador de API (no MVC)
    [Route("api/[controller]")] // Ruta base: /api/products
    [Authorize] // Requiere autenticación JWT para todos los endpoints por defecto
    public class ProductsController : ControllerBase
    {
        // Servicio que maneja la lógica de productos, inyectado por el contenedor
        private readonly IProductsManagementService _productsService;
        //Usar la interfaz es como decir “dame un control remoto,
        //no me importa cómo es el televisor por dentro”.
        // Esto permite que el controlador no dependa de una implementación concreta,

        // Constructor: recibe el servicio mediante inyección de dependencias
        public ProductsController(IProductsManagementService productsService)
        {
            _productsService = productsService;
        }

        // ============================
        // POST /api/products
        // Crea un nuevo producto (solo Admin)
        // ============================
        [HttpPost]
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol Admin pueden crear productos
        public async Task<IActionResult> Create([FromBody] ProductModel.RequestProductModel model)
        {
            // Se delega completamente al servicio; si hay error, el middleware lo manejará
            var created = await _productsService.AddProduct(model);

            // Devuelve 201 Created con la URL del nuevo producto
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ============================
        // GET /api/products
        // Devuelve todos los productos activos
        // ============================
        [HttpGet]
        [AllowAnonymous] // Este endpoint es público: no requiere autenticación
        public async Task<IActionResult> GetAll()
        {
            // Se obtiene la lista completa de productos
            var products = await _productsService.GetAllProducts();

            // Se filtran los productos activos solamente
            var active = products?.Where(p => p.IsActive).ToList();

            // Si no hay productos activos, devolver 204 No Content
            if (active == null || !active.Any())
                return NoContent();

            // Devuelve 200 OK con la lista de productos activos
            return Ok(active);
        }

        // ============================
        // GET /api/products/{id}
        // Devuelve un producto por su ID
        // ============================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Admins y Users pueden ver un producto por ID
        public async Task<IActionResult> GetById(Guid id)
        {
            // Se busca el producto por su ID
            var product = await _productsService.GetProductById(id);

            // Si no existe o está inactivo, devuelve 404 Not Found
            if (product == null || !product.IsActive)
                return NotFound();

            // Devuelve 200 OK con el producto encontrado
            return Ok(product);
        }

        // ============================
        // PUT /api/products/{id}
        // Actualiza un producto existente (solo Admin)
        // ============================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Solo los administradores pueden editar productos
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductModel.RequestProductModel model)
        {
            // Llama al servicio para actualizar el producto
            var updated = await _productsService.UpdateProduct(id, model);

            // Si el producto no existe, devuelve 404 Not Found
            return updated == null ? NotFound() : Ok(updated);
        }

        // ============================
        // PATCH /api/products/{id}
        // Desactiva un producto (soft delete)
        // ============================
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")] // Solo Admin puede deshabilitar productos
        public async Task<IActionResult> Disable(Guid id)
        {
            // Intenta deshabilitar el producto
            var success = await _productsService.DisableProduct(id);

            // Devuelve 204 si tuvo éxito, o 404 si no se encontró
            return success ? NoContent() : NotFound();
        }
    }
}
