using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
    public class BadRequestException : ApplicationException
    {
        // Constructor que recibe un mensaje personalizado y se lo pasa al constructor de la clase base
        public BadRequestException(string message) : base(message) { }
    }
}
