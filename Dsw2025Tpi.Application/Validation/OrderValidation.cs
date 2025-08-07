using Dsw2025Tpi.Application.Exceptions; // ✅ Usamos tus excepciones personalizadas
using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Validation
{
    // Clase estática para validar una solicitud de creación de orden
    public static class OrderValidator
    {
  
        public static void Validate(OrderModel.RequestOrderModel request)
        {
          
            if (request == null)
                throw new BadRequestException("La orden no puede ser nula.");

          
            if (request.CustomerId == Guid.Empty)
                throw new BadRequestException("El cliente es obligatorio.");

      
            if (string.IsNullOrWhiteSpace(request.ShippingAddress) || request.ShippingAddress.Length > 256)
                throw new BadRequestException("La dirección de envío es obligatoria y no puede superar los 256 caracteres.");

           
            if (string.IsNullOrWhiteSpace(request.BillingAddress) || request.BillingAddress.Length > 256)
                throw new BadRequestException("La dirección de facturación es obligatoria y no puede superar los 256 caracteres.");

          
            if (request.OrderItems == null || request.OrderItems.Count == 0)
                throw new BadRequestException("Debe incluir al menos un ítem en la orden.");
        }
    }
}

