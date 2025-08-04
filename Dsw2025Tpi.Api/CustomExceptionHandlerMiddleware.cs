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
        private readonly RequestDelegate _next;

        // Constructor: recibe el "siguiente" middleware en el pipeline HTTP
        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Método principal que intercepta las solicitudes HTTP
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Si todo va bien, continúa al siguiente middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // Si ocurre una excepción, se captura y se maneja de forma controlada
                await HandleExceptionAsync(context, ex);
            }
        }

        // Método auxiliar que traduce las excepciones en respuestas HTTP válidas
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Código de estado HTTP por defecto
            var statusCode = HttpStatusCode.InternalServerError;

            var message = "Se produjo un error interno en el servidor.";

            // Determinar el tipo de excepción para ajustar el mensaje y código de estado
            switch (exception)
            {
                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;

                case EntityNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;

                case DuplicatedEntityException:
                    statusCode = HttpStatusCode.Conflict;
                    message = exception.Message;
                    break;


                default:
                    // Para otros errores no específicos, mostramos el mensaje original
                    message = exception.Message;
                    break;
            }

            // Construimos una respuesta JSON con el mensaje y el código de estado
            var result = JsonSerializer.Serialize(new
            {
                error = message,
                status = (int)statusCode
            });

            // Configuramos la respuesta HTTP
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Enviamos la respuesta
            return context.Response.WriteAsync(result);
        }
    }
}
