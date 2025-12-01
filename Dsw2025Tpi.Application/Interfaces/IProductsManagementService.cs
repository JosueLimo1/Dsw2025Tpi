using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IProductsManagementService
    {
        Task<ProductModel.ResponseProductModel?> GetProductById(Guid id);
        Task<IEnumerable<ProductModel.ResponseProductModel>> GetAllProducts();
        Task<ProductModel.ResponsePagination> GetProducts(ProductModel.FilterProduct request);
        Task<ProductModel.ResponseProductModel> AddProduct(ProductModel.RequestProductModel request);
        Task<ProductModel.ResponseProductModel> UpdateProduct(Guid id, ProductModel.RequestProductModel request);

        // Inhabilita un producto (soft delete) - LO DEJAMOS PARA EL FUTURO BOTÓN EDITAR
        Task<bool> DisableProduct(Guid id);

        // =================================================================
        // NUEVO MÉTODO: Eliminar físicamente de la base de datos
        // =================================================================
        Task<bool> DeleteProduct(Guid id);
    }
}