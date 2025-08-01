using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Validation
{
    public static class ProductValidator
    {
        public static void Validate(ProductModel.RequestProductModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new BadRequestException("El SKU es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.InternalCode))
                throw new BadRequestException("El código interno es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Description))
                throw new BadRequestException("La descripción es obligatoria.");

            if (request.CurrentUnitPrice <= 0)
                throw new BadRequestException("El precio debe ser un valor positivo.");

            if (request.StockQuantity < 0)
                throw new BadRequestException("El stock debe ser un valor positivo.");
        }
    }
}
