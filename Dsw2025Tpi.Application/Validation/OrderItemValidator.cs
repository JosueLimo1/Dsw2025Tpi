using Dsw2025Tpi.Application.Exceptions; // ✅ Usamos tus excepciones personalizadas
using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Validation
{
    // Clase estática para validar ítems de una orden
    public static class OrderItemValidator
    {
        // Valida que el ítem tenga todos los campos requeridos correctamente
        public static void Validate(OrderItemModel.RequestOrderItemModel item)
        {
            // Validamos que el ítem no sea nulo
            if (item == null)
                throw new BadRequestException("El ítem de la orden no puede ser nulo.");

            // Validamos que el ID del producto no sea vacío
            if (item.ProductId == Guid.Empty)
                throw new BadRequestException("El producto es obligatorio.");

            // Validamos que la cantidad solicitada sea mayor a cero
            if (item.Quantity <= 0)
                throw new BadRequestException("La cantidad debe ser mayor a cero.");
        }
    }
}
