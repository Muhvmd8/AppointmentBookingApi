using Microsoft.AspNetCore.Http;
using Shared;
using Shared.Models;

namespace Presentation.Factories
{
    public static class ApiResponseFactory
    {
        public static ObjectResult Problem(
            Result result,
            string traceId)
        {
            var statusCode = result.Error?.ErrorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var title = result.Error?.ErrorType switch
            {
                ErrorType.Validation => "Validation Error",
                ErrorType.NotFound => "Not Found",
                ErrorType.Unauthorized => "Unauthorized",
                ErrorType.Forbidden => "Forbidden",
                ErrorType.Conflict => "Conflict",
                _ => "Internal Server Error"
            };

            var problem = new Dictionary<string, object?>
            {
                ["type"] = $"https://httpstatuses.com/{result.Error?.Code}",
                ["title"] = title,
                ["status"] = statusCode
            };

            if (result.Error?.ErrorType is ErrorType.Validation)
            {
                problem["errors"] = new Dictionary<string, string[]>
                {
                    [result.Error.Code] = [result.Error.Message]
                };
            }
            else
            {
                problem["detail"] = result.Error?.Message;
            }

            problem["traceId"] = traceId;

            return new ObjectResult(problem)
            {
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> Success<T>(
            T data,
            string traceId,
            string? message = null,
            PaginationMeta? pagination = null)
        {
            return ApiResponse<T>.Ok(
                data,
                traceId,
                message,
                pagination);
        }
    }
}
