namespace API.Middlewares.Extensions
{
    public static class RateLimiterMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomRateLimiter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimiterMiddleware>();
        }
    }
}
