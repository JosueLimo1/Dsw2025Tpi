using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

// Define el namespace donde se encuentra el servicio
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
                throw new ArgumentException("El SKU es obligatorio.");

            // Verifica que el SKU sea único
            if (await _context.Products.AnyAsync(p => p.Sku == request.Sku))
                throw new InvalidOperationException("El SKU ya existe.");

            // Valida que el nombre del producto no esté vacío
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("El nombre es obligatorio.");

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
                // Genera manualmente el Id del producto
                Id = Guid.NewGuid()
            };

            // Agrega el producto a la base de datos
            _context.Products.Add(product);

            // Guarda los cambios en la base
            await _context.SaveChangesAsync();

            // Devuelve el producto recién creado en formato de respuesta
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
            // Obtiene los productos que están marcados como activos
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            // Mapea cada producto a su DTO de respuesta
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

        // Método que devuelve un producto específico por ID
        public async Task<ProductModel.ResponseProductModel?> GetProductById(Guid id)
        {
            // Busca el producto en la base por su ID
            var p = await _context.Products.FindAsync(id);
            if (p == null) return null;

            // Devuelve el producto en formato DTO
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
            if (p == null) return null;

            // Valida que el SKU no esté vacío
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new ArgumentException("El SKU es obligatorio.");

            // Valida que el nombre no esté vacío
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("El nombre es obligatorio.");

            // Si cambió el SKU, verifica que el nuevo no esté duplicado
            if (p.Sku != request.Sku && await _context.Products.AnyAsync(prod => prod.Sku == request.Sku))
                throw new InvalidOperationException("Otro producto ya tiene ese SKU.");

            // Asigna los nuevos valores al producto
            p.Sku = request.Sku;
            p.InternalCode = request.InternalCode;
            p.Name = request.Name;
            p.Description = request.Description;
            p.CurrentUnitPrice = request.CurrentUnitPrice;
            p.StockQuantity = request.StockQuantity;
            p.IsActive = request.IsActive;

            // Guarda los cambios en la base
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

        // Método para inhabilitar (desactivar) un producto
        public async Task<bool> DisableProduct(Guid id)
        {
            // Busca el producto por ID
            var p = await _context.Products.FindAsync(id);
            if (p == null) return false;

            // Marca el producto como inactivo
            p.IsActive = false;

            // Guarda los cambios
            await _context.SaveChangesAsync();

            // Devuelve true si fue exitoso
            return true;
        }
    }
}


