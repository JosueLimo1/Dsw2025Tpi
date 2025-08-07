using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Api.Middleware
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Se produjo un error interno en el servidor.";

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
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;

                default:
                    message = exception.Message;
                    break;
            }

            var result = JsonSerializer.Serialize(new
            {
                error = message,
                status = (int)statusCode
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(result);
        }
    }
}
