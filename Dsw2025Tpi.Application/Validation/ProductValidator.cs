using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Validation
{
    // Clase estática responsable de validar instancias de productos
    public static class ProductValidator
    {

        public static void Validate(Product product)
        {
            // Validamos que el producto no sea nulo
            if (product == null)
                throw new EntityNotFoundException("El producto no puede ser nulo.");

            // Validamos que el producto esté activo
            if (!product.IsActive)
                throw new BadRequestException("El producto no está activo.");

            // Validamos que el precio unitario actual sea mayor a 0
            if (product.CurrentUnitPrice <= 0)
                throw new BadRequestException("El precio debe ser mayor a 0.");

            // Validamos que el stock no sea negativo
            if (product.StockQuantity < 0)
                throw new BadRequestException("La cantidad de stock no puede ser negativa.");
        }
        public static void EnsureStock(Product product, int requestedQuantity)
        {
            // Si el stock disponible es menor que la cantidad pedida, lanzamos error
            if (product.StockQuantity < requestedQuantity)
                throw new BadRequestException("No hay stock suficiente para este producto.");
        }
    }
}
