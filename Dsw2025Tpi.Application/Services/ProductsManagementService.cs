using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Validation;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService : IProductsManagementService
    {
        // Repositorio inyectado para acceder a la base de datos
        private readonly IRepository _repository;

        // Constructor que recibe el repositorio por inyección de dependencias
        public ProductsManagementService(IRepository repository)
        {
            _repository = repository;
        }

        // Método que busca un producto por su ID y lo devuelve como DTO de respuesta
        public async Task<ProductModel.ResponseProductModel?> GetProductById(Guid id)
        {
            // Busca el producto por su ID
            var product = await _repository.GetById<Product>(id);

            // Si no se encuentra, lanza excepción
            if (product == null)
                throw new EntityNotFoundException("Producto no encontrado");

            // Si se encuentra, lo transforma a un DTO de respuesta y lo retorna
            return product != null ?
                new ProductModel.ResponseProductModel(
                    product.Id,
                    product.Sku,
                    product.InternalCode,
                    product.Name,
                    product.Description,
                    product.CurrentUnitPrice,
                    product.StockQuantity,
                    product.IsActive
                ) : null;
        }

        // Método que devuelve todos los productos activos como una lista de DTOs
        public async Task<IEnumerable<ProductModel.ResponseProductModel>?> GetAllProducts()
        {
            // Obtiene todos los productos cuyo campo IsActive sea true
            var products = await _repository.GetFiltered<Product>(p => p.IsActive);

            // Mapea cada entidad a un DTO de respuesta
            return products?.Select(p => new ProductModel.ResponseProductModel(
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

        // Método que agrega un nuevo producto al sistema
        public async Task<ProductModel.ResponseProductModel> AddProduct(ProductModel.RequestProductModel request)
        {
            // Valida el contenido del DTO de entrada
            ProductValidator.Validate(request);

            // Verifica si ya existe un producto con el mismo SKU
            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null)
                throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");

            // Crea una nueva instancia de producto con los datos del request
            var product = new Product(
                request.Sku,
                request.InternalCode,
                request.Name,
                request.Description,
                request.CurrentUnitPrice,
                request.StockQuantity,
                request.IsActive
            );

            // Guarda el nuevo producto en la base de datos
            await _repository.Add(product);

            // Devuelve el producto creado como DTO de respuesta
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

        // Método que actualiza un producto existente, identificándolo por su ID
        public async Task<ProductModel.ResponseProductModel> UpdateProduct(Guid id, ProductModel.RequestProductModel request)
        {
            // Busca el producto en la base de datos
            var exist = await _repository.GetById<Product>(id);
            if (exist == null)
                throw new EntityNotFoundException("Producto no encontrado");

            // Valida el DTO de entrada
            ProductValidator.Validate(request);

            // Actualiza los campos editables del producto
            exist.Sku = request.Sku;
            exist.InternalCode = request.InternalCode;
            exist.Name = request.Name;
            exist.Description = request.Description;
            exist.CurrentUnitPrice = request.CurrentUnitPrice;
            exist.StockQuantity = request.StockQuantity;
            exist.IsActive = request.IsActive;

            // Guarda los cambios en la base de datos
            await _repository.Update(exist);

            // Devuelve el producto actualizado como DTO de respuesta
            return new ProductModel.ResponseProductModel(
                exist.Id,
                exist.Sku,
                exist.InternalCode,
                exist.Name,
                exist.Description,
                exist.CurrentUnitPrice,
                exist.StockQuantity,
                exist.IsActive
            );
        }

        // Método que desactiva un producto (cambia su estado activo a false)
        public async Task<ProductModel.ResponseProductModel> PatchProduct(Guid id)
        {
            // Busca el producto por su ID
            var exist = await _repository.GetById<Product>(id);
            if (exist == null)
                throw new EntityNotFoundException("Producto no encontrado.");

            // Cambia su estado activo a falso
            exist.IsActive = false;

            // Guarda los cambios
            await _repository.Update(exist);

            // Devuelve el producto actualizado como DTO de respuesta
            return new ProductModel.ResponseProductModel(
                exist.Id,
                exist.Sku,
                exist.InternalCode,
                exist.Name,
                exist.Description,
                exist.CurrentUnitPrice,
                exist.StockQuantity,
                exist.IsActive
            );
        }
    }
}
