using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record ClientRegisterModel(
        string Username,
        string Email,
        string Password,
        string Name,
        string PhoneNumber
    );
}

