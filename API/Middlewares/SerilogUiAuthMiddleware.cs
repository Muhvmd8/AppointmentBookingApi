namespace API.Middlewares
{
    public class SerilogUiAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogUiAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/serilog-ui") &&
                !context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Serilog UI\"";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await _next(context);
        }
    }
}
