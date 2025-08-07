using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Api.Middleware
{
    // Middleware personalizado para manejar excepciones globales
    public class CustomExceptionHandlerMiddleware
    {
        // Guarda el siguiente paso en la cadena de procesamiento de la solicitud HTTP
        private readonly RequestDelegate _next;

        // Constructor: recibe ese "siguiente paso" y lo guarda para usarlo más adelante
        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        // Método principal que intercepta todas las solicitudes HTTP
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Intenta continuar con el siguiente middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // Si ocurre una excepción, la maneja globalmente
                await HandleExceptionAsync(context, ex);
            }
        }

        // Método auxiliar que traduce las excepciones en respuestas HTTP válidas y estructuradas
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Código de estado por defecto: error interno del servidor
            var statusCode = HttpStatusCode.InternalServerError;

            // Mensaje por defecto
            var message = "Se produjo un error interno en el servidor.";

            // Se analiza el tipo de excepción para asignar el código HTTP adecuado
            switch (exception)
            {
                case BadRequestException:
                    // Error de validación de datos o reglas de negocio
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;

                case EntityNotFoundException:
                    // Recurso no encontrado
                    statusCode = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;

                case DuplicatedEntityException:
                    // ❗ Por lineamiento del TPI, lo tratamos como BadRequest, no como Conflict (409)
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;

                default:
                    // Para otros errores no previstos, se devuelve el mensaje genérico
                    message = exception.Message;
                    break;
            }

            // Serializa la respuesta como JSON con el código y mensaje de error
            var result = JsonSerializer.Serialize(new
            {
                error = message,
                status = (int)statusCode
            });

            // Se configura la respuesta HTTP con el código y tipo de contenido
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Se envía la respuesta al cliente
            return context.Response.WriteAsync(result);
        }
    }
}
