using System.Net;

namespace Shared.Exceptions
{
    public class ConflictException : ApiException
    {
        public ConflictException(string message) : base(message, (int)HttpStatusCode.Conflict) { }
    }
}