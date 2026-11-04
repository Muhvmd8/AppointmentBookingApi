using System.Net;

namespace Shared.Exceptions
{
    public class InternalServerException : ApiException
    {
        public InternalServerException(string message)
            : base(message, (int)HttpStatusCode.InternalServerError) { }
    }
}
