using Dsw2025Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Validation
{
    public static class CustomerValidator
    {
        public static void Validate(CustomerModel.RequestCustomer request)
        {
            if (request == null)
                throw new InvalidOperationException("El cliente no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("El email es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new InvalidOperationException("El teléfono es obligatorio.");
        }
    }
}
