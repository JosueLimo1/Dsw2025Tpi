using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Validation
{
    public static class ProductValidator
    {
        public static void Validate(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product), "El producto no puede ser nulo.");

            if (!product.IsActive)
                throw new InvalidOperationException("El producto no está activo.");

            if (product.CurrentUnitPrice <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0.");

            if (product.StockQuantity < 0)
                throw new ArgumentException("La cantidad de stock no puede ser negativa.");
        }

        public static void EnsureStock(Product product, int requestedQuantity)
        {
            if (product.StockQuantity < requestedQuantity)
                throw new InvalidOperationException("No hay stock suficiente para este producto.");
        }
    }
}
