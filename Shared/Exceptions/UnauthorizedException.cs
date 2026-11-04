using System.Net;

namespace Shared.Exceptions
{
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message) : base(message, (int)HttpStatusCode.Unauthorized) { }
    }
}