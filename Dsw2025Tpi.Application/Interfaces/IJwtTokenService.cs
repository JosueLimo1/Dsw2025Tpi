using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(Customer customer);
    }
}

