using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsManagementService _productsService;

        public ProductsController(IProductsManagementService productsService)
        {
            _productsService = productsService;
        }

        // POST: api/products (Crear)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ProductModel.RequestProductModel model)
        {
            var created = await _productsService.AddProduct(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // GET: api/products (Listar activos - Público)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productsService.GetAllProducts();
            var active = products?.Where(p => p.IsActive).ToList();

            if (active == null || !active.Any())
                return NoContent();

            return Ok(active);
        }

        // GET: api/products/{id} (Obtener uno)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productsService.GetProductById(id);
            if (product == null || !product.IsActive) return NotFound();
            return Ok(product);
        }

        // PUT: api/products/{id} (Actualizar)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductModel.RequestProductModel model)
        {
            var updated = await _productsService.UpdateProduct(id, model);
            return updated == null ? NotFound() : Ok(updated);
        }

        // ================================================================
        // DELETE: api/products/{id}
        // ================================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // CAMBIO: Ahora llamamos a DeleteProduct (Borrado Físico)
            var success = await _productsService.DeleteProduct(id);

            // Si tuvo éxito, devolvemos 204 No Content
            // Si no encontró el ID, devolvemos 404 NotFound
            return success ? NoContent() : NotFound("Producto no encontrado.");
        }

        // PATCH: api/products/{id} (Desactivar - Endpoint alternativo)
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Disable(Guid id)
        {
            var success = await _productsService.DisableProduct(id);
            return success ? NoContent() : NotFound();
        }

        // GET: api/products/admin (Listado avanzado paginado)
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuthProducts([FromQuery] ProductModel.FilterProduct request)
        {
            var products = await _productsService.GetProducts(request);

            if (products == null || products.ProductItems.Count == 0)
            {
                Response.Headers.Append("X-Message", "There are no active products");
                return NoContent();
            }

            return Ok(products);
        }
    }
}