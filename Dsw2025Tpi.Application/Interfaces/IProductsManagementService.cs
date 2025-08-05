using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Interfaces
{
    // Interfaz que define los métodos disponibles para manejar productos
    public interface IProductsManagementService
    {
        // Devuelve un producto por ID
        Task<ProductModel.ResponseProductModel?> GetProductById(Guid id);

        // Devuelve todos los productos registrados
        Task<IEnumerable<ProductModel.ResponseProductModel>> GetAllProducts();

        // Agrega un nuevo producto al sistema
        Task<ProductModel.ResponseProductModel> AddProduct(ProductModel.RequestProductModel request);

        // Actualiza un producto existente
        Task<ProductModel.ResponseProductModel> UpdateProduct(Guid id, ProductModel.RequestProductModel request);

        // Inhabilita un producto (soft delete)
        Task<bool> DisableProduct(Guid id);
    }
}
