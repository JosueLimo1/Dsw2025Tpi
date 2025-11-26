using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record ProductModel
    {
        // ========================
        // Crear producto
        // ========================
        public record RequestProductModel(
            string Sku,
            string InternalCode,
            string Name,
            string Description,
            decimal CurrentUnitPrice,
            int StockQuantity,
            bool IsActive);

        // ========================
        // Respuesta de producto
        // ========================
        public record ResponseProductModel(
            Guid Id,
            string Sku,
            string InternalCode,
            string Name,
            string Description,
            decimal CurrentUnitPrice,
            int StockQuantity,
            bool IsActive);

        // ========================
        // Filtro + paginación
        // ========================
        public record FilterProduct(
            string? Status,       // "enabled", "disabled" o null
            string? Search,       // búsqueda por nombre o código
            int? PageNumber,      // número de página
            int? PageSize         // tamaño de página
        );

        // ========================
        // Respuesta paginada
        // ========================
        public record ResponsePagination(
            List<ResponseProductModel> ProductItems,
            int Total
        );
    }
}
