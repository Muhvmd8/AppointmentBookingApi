using System.Net;

namespace Shared.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message)
            : base(message, (int)HttpStatusCode.NotFound) { }
    }
}
