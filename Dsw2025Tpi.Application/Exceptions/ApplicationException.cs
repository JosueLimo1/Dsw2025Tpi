using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions;
public class ApplicationException : Exception
{
    //Permite crear una excepción con un mensaje de error personalizado.
    public ApplicationException(string message): base(message) { }

    //Permite crear una excepción con un mensaje de error personalizado y una excepción interna.
    public ApplicationException(string message, Exception innerException)
       : base(message, innerException) { }
}
    //Esto se usa para encadenar errores: por ejemplo, si una excepción ocurre dentro de otra, podés conservar ambas.
