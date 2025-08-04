using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Validation
{
    public static class CustomerValidator
    {
        public static void Validate(Customer? customer)
        {
            if (customer == null)
                throw new ArgumentException("Cliente no encontrado.");

            if (string.IsNullOrWhiteSpace(customer.Name))
                throw new ArgumentException("El nombre del cliente no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new ArgumentException("El email del cliente no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
                throw new ArgumentException("El número de teléfono del cliente no puede estar vacío.");
        }
    }
}
