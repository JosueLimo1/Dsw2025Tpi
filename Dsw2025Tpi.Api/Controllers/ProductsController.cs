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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ProductModel.RequestProductModel model)
        {
            var created = await _productsService.AddProduct(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

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

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productsService.GetProductById(id);
            if (product == null || !product.IsActive)
                return NotFound();

            return Ok(product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductModel.RequestProductModel model)
        {
            var updated = await _productsService.UpdateProduct(id, model);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Disable(Guid id)
        {
            var success = await _productsService.DisableProduct(id);
            return success ? NoContent() : NotFound();
        }
    }
}