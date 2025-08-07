using Dsw2025Tpi.Application.Exceptions; // ✅ Importamos tus excepciones personalizadas
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Validation
{
    // Clase estática para validar instancias de Customer
    public static class CustomerValidator
    {
        // Método que valida un objeto Customer. Lanza excepciones personalizadas si hay errores.
        public static void Validate(Customer? customer)
        {
            // Si el cliente es nulo, lanzamos una excepción de entidad no encontrada (404)
            if (customer == null)
                throw new EntityNotFoundException("Cliente no encontrado.");

            // Validamos que el nombre no esté vacío o nulo
            if (string.IsNullOrWhiteSpace(customer.Name))
                throw new BadRequestException("El nombre del cliente no puede estar vacío.");

            // Validamos que el email no esté vacío o nulo
            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new BadRequestException("El email del cliente no puede estar vacío.");

            // Validamos que el número de teléfono no esté vacío o nulo
            if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
                throw new BadRequestException("El número de teléfono del cliente no puede estar vacío.");
        }
    }
}

