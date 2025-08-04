using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService : IProductsManagementService
    {
        private readonly Dsw2025TpiContext _context;

        public ProductsManagementService(Dsw2025TpiContext context)
        {
            _context = context;
        }

        // 1. Crear un producto nuevo con validaciones del enunciado
        public async Task<ProductModel.ResponseProductModel?> AddProduct(ProductModel.RequestProductModel request)
        {
            // Validación: SKU obligatorio
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new ArgumentException("El SKU es obligatorio.");

            // Validación: SKU único
            if (await _context.Products.AnyAsync(p => p.Sku == request.Sku))
                throw new InvalidOperationException("El SKU ya existe.");

            // Validación: nombre obligatorio
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("El nombre es obligatorio.");

            // Crea instancia del producto con constructor protegido por validaciones internas
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
                Id = Guid.NewGuid()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return new ProductModel.ResponseProductModel(
                product.Id, product.Sku, product.InternalCode, product.Name,
                product.Description, product.CurrentUnitPrice,
                product.StockQuantity, product.IsActive);
        }

        // 2. Obtener todos los productos activos
        public async Task<IEnumerable<ProductModel.ResponseProductModel>> GetAllProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            return products.Select(p => new ProductModel.ResponseProductModel(
                p.Id, p.Sku, p.InternalCode, p.Name,
                p.Description, p.CurrentUnitPrice,
                p.StockQuantity, p.IsActive));
        }

        // 3. Obtener producto por ID
        public async Task<ProductModel.ResponseProductModel?> GetProductById(Guid id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return null;

            return new ProductModel.ResponseProductModel(
                p.Id, p.Sku, p.InternalCode, p.Name,
                p.Description, p.CurrentUnitPrice,
                p.StockQuantity, p.IsActive);
        }

        // 4. Actualizar un producto existente
        public async Task<ProductModel.ResponseProductModel?> UpdateProduct(Guid id, ProductModel.RequestProductModel request)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return null;

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new ArgumentException("El SKU es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("El nombre es obligatorio.");

            // Validación: evitar duplicado si cambió el SKU
            if (p.Sku != request.Sku && await _context.Products.AnyAsync(prod => prod.Sku == request.Sku))
                throw new InvalidOperationException("Otro producto ya tiene ese SKU.");

            // Asignación de nuevos valores (validaciones internas se aplican)
            p.Sku = request.Sku;
            p.InternalCode = request.InternalCode;
            p.Name = request.Name;
            p.Description = request.Description;
            p.CurrentUnitPrice = request.CurrentUnitPrice;
            p.StockQuantity = request.StockQuantity;
            p.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return new ProductModel.ResponseProductModel(
                p.Id, p.Sku, p.InternalCode, p.Name,
                p.Description, p.CurrentUnitPrice,
                p.StockQuantity, p.IsActive);
        }

        // 5. Inhabilitar un producto (PATCH)
        public async Task<bool> DisableProduct(Guid id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return false;

            p.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

