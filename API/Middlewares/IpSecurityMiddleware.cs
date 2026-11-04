using Domain.Models.Security;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Service;

namespace API.Middlewares
{
    public class IpSecurityMiddleware
    {
        private readonly RequestDelegate _next;

        public IpSecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext db, IpSecurityService security)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            bool riskDetected = false;
            string? body = null;

            // Check if IP is already banned
            var blocked = await db.BlockedIps.FirstOrDefaultAsync(b =>
                b.IpAddress == ip && (b.ExpiresAt == null || b.ExpiresAt > DateTime.Now));
            if (blocked != null)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Your IP is banned.");
                return;
            }

            // Read request body if available
            if (context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // RULE 1: XSS patterns
            if (security.ContainsMaliciousPayload(body))
            {
                db.BlockedIps.Add(new BlockedIp
                {
                    IpAddress = ip,
                    Reason = "XSS Attempt",
                    ExpiresAt = DateTime.Now.AddMinutes(60)
                });
                riskDetected = true;

                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Your IP was banned due to malicious activity.");
                await db.SaveChangesAsync();
                return;
            }

            // Continue request
            await _next(context);

            // RULE 2: Too many failed logins
            if (await security.TooManyFailedLogins(ip, 5, TimeSpan.FromMinutes(5)))
            {
                db.BlockedIps.Add(new BlockedIp
                {
                    IpAddress = ip,
                    Reason = "Brute Force Detected",
                    ExpiresAt = DateTime.Now.AddMinutes(30)
                });
                riskDetected = true;
            }

            if (riskDetected)
            {
                db.LoginAudits.Add(new LoginAudit
                {
                    Username = context.User?.Identity?.Name ?? "Anonymous",
                    IpAddress = ip,
                    Endpoint = context.Request.Path,
                    Status = context.Response.StatusCode < 400 ? "Success" : "Failed",
                    RequestBody = body
                });

                await db.SaveChangesAsync();
            }
        }
    }

}
