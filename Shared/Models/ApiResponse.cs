namespace Shared.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public T? Data { get; set; }
        public ApiMeta Meta { get; set; } = default!;

        public static ApiResponse<T> Ok(T data, string traceId, string? message = null, PaginationMeta? pagination = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Meta = new ApiMeta
                {
                    TraceId = traceId,
                    Pagination = pagination
                }
            };
        }
    }
}
