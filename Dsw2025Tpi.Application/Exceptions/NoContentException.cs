using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
    // Excepción para indicar que no hay contenido disponible (similar al HTTP 204)
    public class NoContentException : ApplicationException
    {
        public NoContentException(string message)
            : base(message)
        {
        }
    }
}

