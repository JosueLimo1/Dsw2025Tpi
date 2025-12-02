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

            // --- CORRECCIÓN: ELIMINAMOS ESTA VALIDACIÓN ---
            // No debemos validar IsActive aquí, porque si el admin quiere desactivarlo
            // (IsActive = false), esta regla lanzaba error e impedía guardar el cambio.

            /* if (!product.IsActive)
                throw new BadRequestException("El producto no está activo."); 
            */

            // Validamos que el precio unitario actual sea mayor a 0
            // Requerimiento funcional del PDF
            if (product.CurrentUnitPrice <= 0)
                throw new BadRequestException("El precio debe ser mayor a 0.");

            // Validamos que el stock no sea negativo
            // Requerimiento funcional del PDF
            if (product.StockQuantity < 0)
                throw new BadRequestException("La cantidad de stock no puede ser negativa.");
        }

        public static void EnsureStock(Product product, int requestedQuantity)
        {
            // Esta validación es correcta para el proceso de órdenes
            if (product.StockQuantity < requestedQuantity)
                throw new BadRequestException("No hay stock suficiente para este producto.");
        }
    }
}