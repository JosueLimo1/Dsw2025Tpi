using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record CustomerModel
    {
        public record RequestCustomer(string? eMail, string? name, string? phoneNumber);
        public record ResponseCustomer(Guid Id, string? eMail, string? name, string? phoneNumber);
    }
}
