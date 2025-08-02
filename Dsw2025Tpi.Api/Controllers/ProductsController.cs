using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ApplicationException = Dsw2025Tpi.Application.Exceptions.ApplicationException;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/products")]
//[Authorize]
public class ProductsController : ControllerBase
{
    // Inyección del servicio que implementa la lógica de negocio para productos
    private readonly IProductsManagementService _service;

    // Constructor que recibe el servicio inyectado por el contenedor de dependencias
    public ProductsController(IProductsManagementService service)
    {
        _service = service;
    }

    // ───────────────────────────────────────────────────────────────
    // 🔵 1. CREAR UN PRODUCTO
    // POST /api/products
    // Descripción: Crea un nuevo producto en base a los datos recibidos
    // Acceso: Solo rol "Admin"
    // ───────────────────────────────────────────────────────────────
    [HttpPost()]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddProduct([FromBody] ProductModel.RequestProductModel request)
    {
        // Valida el modelo recibido según las anotaciones de datos o reglas externas
        if (!ModelState.IsValid)
            return BadRequest(ModelState); // 400 Bad Request con detalles

        try
        {
            // Llama al servicio para crear el producto
            var product = await _service.AddProduct(request);

            // Retorna 201 Created con la ubicación del recurso creado
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            // Si hubo una excepción de negocio (precio inválido, stock negativo, etc.)
            return BadRequest(new { error = ex.Message });
        }
    }

    // ───────────────────────────────────────────────────────────────
    // 🔵 2. OBTENER TODOS LOS PRODUCTOS
    // GET /api/products
    // Descripción: Retorna una lista completa de productos
    // Acceso: Público (sin autenticación)
    // ───────────────────────────────────────────────────────────────
    [HttpGet()]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllProducts()
    {
        // Obtiene la lista desde el servicio
        var products = await _service.GetAllProducts();

        // Si no hay productos, responde 204 No Content
        if (products == null || !products.Any())
            return NoContent();

        // Devuelve 200 OK con la lista de productos
        return Ok(products);
    }

    // ───────────────────────────────────────────────────────────────
    // 🔵 3. OBTENER UN PRODUCTO POR ID
    // GET /api/products/{id}
    // Descripción: Retorna un producto específico por su ID
    // Acceso: Rol "Admin" o "User"
    // ───────────────────────────────────────────────────────────────
    [HttpGet("{id}")]
    //[Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        // Busca el producto por su ID
        var product = await _service.GetProductById(id);

        // Si no existe, devuelve 404 Not Found
        if (product == null)
            return NotFound();

        // Devuelve 200 OK con los datos del producto
        return Ok(product);
    }

    // ───────────────────────────────────────────────────────────────
    // 🔵 4. ACTUALIZAR UN PRODUCTO
    // PUT /api/products/{id}
    // Descripción: Modifica todos los datos de un producto existente
    // Acceso: Solo rol "Admin"
    // ───────────────────────────────────────────────────────────────
    [HttpPut("{id}")]
   //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.RequestProductModel request)
    {
        // Verifica que el modelo enviado sea válido
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Llama al servicio para actualizar el producto
        var updatedProduct = await _service.UpdateProduct(id, request);

        // Devuelve 200 OK con el producto actualizado
        return Ok(updatedProduct);
    }

    // ───────────────────────────────────────────────────────────────
    // 🔵 5. INHABILITAR UN PRODUCTO
    // PATCH /api/products/{id}
    // Descripción: Cambia el estado de "activo" a "inactivo" o viceversa
    // Acceso: Solo rol "Admin"
    // ───────────────────────────────────────────────────────────────
    [HttpPatch("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchProduct(Guid id)
    {
        // Llama al servicio para alternar el estado activo del producto
        await _service.PatchProduct(id);

        // Devuelve 204 No Content (sin cuerpo)
        return NoContent();
    }
}