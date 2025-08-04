using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(IdentityUser user, IList<string> roles);
    }
}

