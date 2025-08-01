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
        private readonly IRepository _repository;

        public ProductsManagementService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task<ProductModel.ResponseProductModel?> GetProductById(Guid id)
        {
            var product = await _repository.GetById<Product>(id);
            if (product == null)
                throw new EntityNotFoundException("Producto no encontrado");
            return product != null ?
                new ProductModel.ResponseProductModel(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity, product.IsActive) :
                null;
        }

        public async Task<IEnumerable<ProductModel.ResponseProductModel>?> GetAllProducts()
        {
            return (await _repository
                .GetFiltered<Product>(p => p.IsActive))?
                .Select(p => new ProductModel.ResponseProductModel(p.Id, p.Sku, p.InternalCode, p.Name, p.Description,
                p.CurrentUnitPrice, p.StockQuantity, p.IsActive));
        }

        public async Task<ProductModel.ResponseProductModel> AddProduct(ProductModel.RequestProductModel request)
        {
            ProductValidator.Validate(request);
            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
            var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentUnitPrice, request.StockQuantity, request.IsActive);
            await _repository.Add(product);
            return new ProductModel.ResponseProductModel(product.Id, product.Sku, product.InternalCode, product.Name, product.Description,
                product.CurrentUnitPrice, product.StockQuantity, product.IsActive);
        }
        //Como actualizar un producto por id y no por sku, en caso de que lo hagamos por sku, habria que arreglar la ultima parte para que no actualice el id cada vez que haya cambios
        public async Task<ProductModel.ResponseProductModel> UpdateProduct(Guid id, ProductModel.RequestProductModel request)
        {


            var exist = await _repository.GetById<Product>(id);
            if (exist == null)
                throw new EntityNotFoundException("Producto no encontrado");
            ProductValidator.Validate(request);


            // Actualiza solo las propiedades necesarias, el Id no se toca
            exist.Sku = request.Sku;
            exist.InternalCode = request.InternalCode;
            exist.Name = request.Name;
            exist.Description = request.Description;
            exist.CurrentUnitPrice = request.CurrentUnitPrice;
            exist.StockQuantity = request.StockQuantity;
            exist.IsActive = request.IsActive;

            await _repository.Update(exist);

            return new ProductModel.ResponseProductModel
           (
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

        public async Task<ProductModel.ResponseProductModel> PatchProduct(Guid id)
        {
            var exist = await _repository.GetById<Product>(id);
            if (exist == null)
                throw new EntityNotFoundException("Producto no encontrado.");

            exist.IsActive = false; // O alternar: exist.IsActive = !exist.IsActive;
            await _repository.Update(exist);
            var active = await _repository.GetById<Product>(id);

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
