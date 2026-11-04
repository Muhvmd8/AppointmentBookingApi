using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Text.Json;

namespace API.Middlewares
{
    public class RateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;

        public RateLimiterMiddleware(RequestDelegate next, IMemoryCache memoryCache)
        {
            _next = next;
            _memoryCache = memoryCache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/serilog-ui"))
            {
                await _next(context);
                return;
            }

            // Get client IP
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                     ?? context.Connection.RemoteIpAddress?.ToString()
                     ?? "unknown";

            // Check if IP is blocked
            if (_memoryCache.TryGetValue(ip, out _))
            {
                Log.Warning("RateLimit BLOCKED | IP: {Ip} | Endpoint: {Endpoint}", ip, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                var response = JsonSerializer.Serialize(new { Message = "Too many requests. Try again after 5 minutes." });
                await context.Response.WriteAsync(response);
                return;
            }

            // Continue pipeline
            await _next(context);

            // If response was 429, block IP for 5 minutes
            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
                _memoryCache.Set(ip, true, TimeSpan.FromMinutes(5));
                //Log.Warning("RateLimit EXCEEDED | IP: {Ip} | Endpoint: {Endpoint}", ip, context.Request.Path);
            }
        }
    }
}
