using Dsw2025Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IAuthenticateService
    {
        Task<string> LoginAsync(LoginModel model);
        Task<string> RegisterAsync(RegisterModel model);
        Task<string> ClientRegisterAsync(ClientRegisterModel model);
    }
}
