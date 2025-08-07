using Dsw2025Tpi.Application.Exceptions; // ✅ Usamos tus excepciones personalizadas
using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Validation
{
    
    public static class OrderItemValidator
    {
       
        public static void Validate(OrderItemModel.RequestOrderItemModel item)
        {
           
            if (item == null)
                throw new BadRequestException("El ítem de la orden no puede ser nulo.");

            
            if (item.ProductId == Guid.Empty)
                throw new BadRequestException("El producto es obligatorio.");

            
            if (item.Quantity <= 0)
                throw new BadRequestException("La cantidad debe ser mayor a cero.");
        }
    }
}
