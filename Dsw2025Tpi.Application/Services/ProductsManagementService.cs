using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Dsw2025Tpi.Application.Validation;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService : IProductsManagementService
    {
        private readonly Dsw2025TpiContext _context;

        public ProductsManagementService(Dsw2025TpiContext context)
        {
            _context = context;
        }

        public async Task<ProductModel.ResponseProductModel?> AddProduct(ProductModel.RequestProductModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new BadRequestException("El SKU es obligatorio.");

            if (await _context.Products.AnyAsync(p => p.Sku == request.Sku))
                throw new DuplicatedEntityException("El SKU ya existe.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("El nombre es obligatorio.");

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

            ProductValidator.Validate(product);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

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

        public async Task<IEnumerable<ProductModel.ResponseProductModel>> GetAllProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

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

        public async Task<ProductModel.ResponseProductModel?> GetProductById(Guid id)
        {
            var p = await _context.Products.FindAsync(id);

            if (p == null) return null;

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

        public async Task<ProductModel.ResponseProductModel?> UpdateProduct(Guid id, ProductModel.RequestProductModel request)
        {
            var p = await _context.Products.FindAsync(id);

            if (p == null) return null;

            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new BadRequestException("El SKU es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("El nombre es obligatorio.");

            if (p.Sku != request.Sku && await _context.Products.AnyAsync(prod => prod.Sku == request.Sku))
                throw new DuplicatedEntityException("Otro producto ya tiene ese SKU.");

            p.Sku = request.Sku;
            p.InternalCode = request.InternalCode;
            p.Name = request.Name;
            p.Description = request.Description;
            p.CurrentUnitPrice = request.CurrentUnitPrice;
            p.StockQuantity = request.StockQuantity;
            p.IsActive = request.IsActive;

            ProductValidator.Validate(p);

            await _context.SaveChangesAsync();

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