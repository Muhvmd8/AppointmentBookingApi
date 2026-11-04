using Serilog.Ui.Core.Interfaces;
using System.Text;

namespace API.AuthFilters
{
    public class SerilogUiAuthorizationFilter : IUiAsyncAuthorizationFilter
    {
        private readonly string _username;
        private readonly string _password;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SerilogUiAuthorizationFilter(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _username = configuration["SerilogUiSettings:Username"]??String.Empty;
            _password = configuration["SerilogUiSettings:Password"]??String.Empty;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> AuthorizeAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Task.FromResult(false);

            if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Serilog UI\"";
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.FromResult(false);
            }

            if (authHeader.ToString().StartsWith("Basic "))
            {
                var encodedUsernamePassword = authHeader.ToString()["Basic ".Length..].Trim();
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var parts = decodedUsernamePassword.Split(':');

                if (parts.Length == 2)
                {
                    var username = parts[0];
                    var password = parts[1];

                    if (username == _username && password == _password)
                        return Task.FromResult(true);
                }
            }

            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Serilog UI\"";
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.FromResult(false);
        }
    }
}
