namespace API.Middlewares.Extensions
{
    public static class SerilogUiAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseSerilogUiAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerilogUiAuthMiddleware>();
        }
    }
}
