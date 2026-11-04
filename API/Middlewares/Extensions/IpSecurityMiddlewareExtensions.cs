namespace API.Middlewares.Extensions
{
    public static class IpSecurityMiddlewareExtensions
    {
        public static IApplicationBuilder UseIpSecurity(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IpSecurityMiddleware>();
        }
    }
}
