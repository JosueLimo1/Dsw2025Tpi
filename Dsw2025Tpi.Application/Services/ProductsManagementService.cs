using Dsw2025Tpi.Application.Dtos; // Importa los modelos de solicitud y respuesta
using Dsw2025Tpi.Application.Exceptions; // ✅ Importa las excepciones personalizadas
using Dsw2025Tpi.Application.Interfaces; // Importa la interfaz que implementa este servicio
using Dsw2025Tpi.Data; // Acceso al contexto de base de datos
using Dsw2025Tpi.Domain.Entities; // Importa la entidad Product
using Microsoft.EntityFrameworkCore; // Para operaciones asincrónicas con la base de datos
using Dsw2025Tpi.Application.Validation;

namespace Dsw2025Tpi.Application.Services
{
    // Implementación concreta del servicio de productos, basada en la interfaz IProductsManagementService
    public class ProductsManagementService : IProductsManagementService
    {
        // Campo privado para acceder al contexto de base de datos
        private readonly Dsw2025TpiContext _context;

        // Constructor que recibe el contexto por inyección de dependencias
        public ProductsManagementService(Dsw2025TpiContext context)
        {
            _context = context;
        }

        // Método para agregar un nuevo producto a la base de datos
        public async Task<ProductModel.ResponseProductModel?> AddProduct(ProductModel.RequestProductModel request)
        {
            // Valida que el SKU no esté vacío
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new BadRequestException("El SKU es obligatorio."); // ⛔ Excepción personalizada

            // Verifica que el SKU no esté duplicado
            if (await _context.Products.AnyAsync(p => p.Sku == request.Sku))
                throw new DuplicatedEntityException("El SKU ya existe."); // ⛔ Excepción personalizada

            // Valida que el nombre del producto no esté vacío
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("El nombre es obligatorio."); // ⛔ Excepción personalizada

            // Crea una nueva instancia de Product con los datos del request
            var product = new Product(
                sku: request.Sku,
                internalCode: request.InternalCode,
                name: request.Name,
                description: request.Description,
                currentUnitPrice: request.CurrentUnitPrice,
                stockQuantity: request.StockQuantity,
                isActive: request.IsActive
            )
            {
                // Genera automaticamente el Id del producto
                Id = Guid.NewGuid()
            };

            // 🔸 VALIDACIÓN: Producto válido según tus reglas de negocio
            ProductValidator.Validate(product);

            // Agrega el producto al DbSet
            _context.Products.Add(product);

            // Guarda los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Devuelve el producto recién creado como un DTO de respuesta
            return new ProductModel.ResponseProductModel(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentUnitPrice,
                product.StockQuantity,
                product.IsActive
            );
        }

        // Método que devuelve todos los productos activos
        public async Task<IEnumerable<ProductModel.ResponseProductModel>> GetAllProducts()
        {
            // Consulta los productos activos en la base
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            // Transforma la lista de entidades en una lista de DTOs
            return products.Select(p => new ProductModel.ResponseProductModel(
                p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive
            ));
        }

        // Método que devuelve un producto específico por su ID
        public async Task<ProductModel.ResponseProductModel?> GetProductById(Guid id)
        {
            // Busca el producto por ID
            var p = await _context.Products.FindAsync(id);

            // Si no se encuentra, devuelve null
            if (p == null) return null;

            // Devuelve los datos del producto como un DTO
            return new ProductModel.ResponseProductModel(
                p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive
            );
        }

        // Método para actualizar un producto existente
        public async Task<ProductModel.ResponseProductModel?> UpdateProduct(Guid id, ProductModel.RequestProductModel request)
        {
            // Busca el producto por ID
            var p = await _context.Products.FindAsync(id);

            // Si no se encuentra, devuelve null
            if (p == null) return null;

            // Valida que el SKU no esté vacío
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new BadRequestException("El SKU es obligatorio."); // ⛔ Excepción personalizada

            // Valida que el nombre no esté vacío
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("El nombre es obligatorio."); // ⛔ Excepción personalizada

            // Si el SKU fue modificado, verifica que no esté duplicado
            if (p.Sku != request.Sku && await _context.Products.AnyAsync(prod => prod.Sku == request.Sku))
                throw new DuplicatedEntityException("Otro producto ya tiene ese SKU."); // ⛔ Excepción personalizada

            // Asigna los nuevos valores al producto
            p.Sku = request.Sku;
            p.InternalCode = request.InternalCode;
            p.Name = request.Name;
            p.Description = request.Description;
            p.CurrentUnitPrice = request.CurrentUnitPrice;
            p.StockQuantity = request.StockQuantity;
            p.IsActive = request.IsActive;


            // 🔸 VALIDACIÓN: Producto válido luego de la actualización
            ProductValidator.Validate(p);


            // Guarda los cambios
            await _context.SaveChangesAsync();

            // Devuelve el producto actualizado como DTO
            return new ProductModel.ResponseProductModel(
                p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive
            );
        }

        // Método para desactivar un producto (soft delete)
        public async Task<bool> DisableProduct(Guid id)
        {
            // Busca el producto por ID
            var p = await _context.Products.FindAsync(id);

            // Si no se encuentra, devuelve false
            if (p == null) return false;

            // Marca el producto como inactivo
            p.IsActive = false;

            // SI NOS PIDE BORRAR DE LA BD 
            // _context.Products.Remove(p);

            // Guarda los cambios
            await _context.SaveChangesAsync();

            // Devuelve true indicando éxito
            return true;
        }
    }
}


