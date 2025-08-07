using Dsw2025Tpi.Application.Exceptions; 
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Validation
{
 
    public static class CustomerValidator
    {
       
        public static void Validate(Customer? customer)
        {
           
            if (customer == null)
                throw new EntityNotFoundException("Cliente no encontrado.");

           
            if (string.IsNullOrWhiteSpace(customer.Name))
                throw new BadRequestException("El nombre del cliente no puede estar vacío.");

            
            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new BadRequestException("El email del cliente no puede estar vacío.");

            
            if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
                throw new BadRequestException("El número de teléfono del cliente no puede estar vacío.");
        }
    }
}

