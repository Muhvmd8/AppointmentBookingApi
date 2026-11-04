using Domain.Contracts;
using Domain.Models.Security;
using Microsoft.Extensions.Logging;

namespace Service
{
    public class IpSecurityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IpSecurityService> _logger;

        public IpSecurityService(IUnitOfWork unitOfWork, ILogger<IpSecurityService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> TooManyFailedLogins(string ip, int maxAttempts, TimeSpan window)
        {
            var cutoff = DateTime.Now - window;
            var failed = await _unitOfWork.GetRepository<LoginAudit,int>()
                .CountAsync(x => x.IpAddress == ip && x.Status == "Failed" && x.CreatedAt >= cutoff);

            return failed >= maxAttempts;
        }

        public bool ContainsMaliciousPayload(string? body)
        {
            if (string.IsNullOrWhiteSpace(body)) return false;

            string[] xssPatterns = { "<script", "alert(" };

            body = body.ToLower();

            return xssPatterns.Any(body.Contains);
        }
    }

}
