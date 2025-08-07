using Dsw2025Tpi.Application.Exceptions; // ✅ Usamos tus excepciones personalizadas
using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Validation
{
    // Clase estática para validar una solicitud de creación de orden
    public static class OrderValidator
    {
        // Método que valida el objeto RequestOrderModel
        public static void Validate(OrderModel.RequestOrderModel request)
        {
            // Validación general: el request no puede ser nulo
            if (request == null)
                throw new BadRequestException("La orden no puede ser nula.");

            // El cliente debe estar especificado (Guid válido)
            if (request.CustomerId == Guid.Empty)
                throw new BadRequestException("El cliente es obligatorio.");

            // La dirección de envío debe estar presente y tener como máximo 256 caracteres
            if (string.IsNullOrWhiteSpace(request.ShippingAddress) || request.ShippingAddress.Length > 256)
                throw new BadRequestException("La dirección de envío es obligatoria y no puede superar los 256 caracteres.");

            // La dirección de facturación debe estar presente y tener como máximo 256 caracteres
            if (string.IsNullOrWhiteSpace(request.BillingAddress) || request.BillingAddress.Length > 256)
                throw new BadRequestException("La dirección de facturación es obligatoria y no puede superar los 256 caracteres.");

            // Debe haber al menos un ítem en la orden
            if (request.OrderItems == null || request.OrderItems.Count == 0)
                throw new BadRequestException("Debe incluir al menos un ítem en la orden.");
        }
    }
}

