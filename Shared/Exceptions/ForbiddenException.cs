using System.Net;

namespace Shared.Exceptions
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message)
            : base(message, (int)HttpStatusCode.Forbidden) { }
    }
}
