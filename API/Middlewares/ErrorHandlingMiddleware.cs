using Shared.Exceptions;
using System.Text;
using System.Text.Json;

namespace API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            // Buffer the response so we can inspect it after other middleware wrote to it.
            await using var responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;

            try
            {
                await _next(context);

                // After pipeline executed, inspect buffered body
                responseBuffer.Position = 0;
                string bodyText = await new StreamReader(responseBuffer, Encoding.UTF8).ReadToEndAsync();

                // If response is 401 or 403 and body is empty -> produce unified error JSON
                if ((context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
                     context.Response.StatusCode == StatusCodes.Status403Forbidden) &&
                    string.IsNullOrWhiteSpace(bodyText) &&
                    !context.Response.HasStarted)
                {
                    var message = context.Response.StatusCode == StatusCodes.Status401Unauthorized
                        ? "Unauthorized"
                        : "Forbidden";

                    var errorResponse = new
                    {
                        message
                    };
                    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength = bytes.Length;

                    // Restore original stream and write our JSON
                    context.Response.Body = originalBodyStream;
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    return;
                }

                // Otherwise copy the original buffered response back to the real response stream
                responseBuffer.Position = 0;
                context.Response.Body = originalBodyStream;
                await responseBuffer.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                // Ensure we restore the original stream before writing the error
                context.Response.Body = originalBodyStream;
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode = StatusCodes.Status500InternalServerError;
            string message = "An unexpected error occurred.";

            if (exception is ApiException apiEx)
            {
                statusCode = apiEx.StatusCode;
                message = apiEx.Message;
            }
            else
            {
                // You can change this to a generic message in production and log the real exception.
                message = exception.Message;
            }

            var response = new { message, (exception as ApiException)?.Errors };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, jsonOptions);

            await context.Response.WriteAsync(json);
        }

    }
}
