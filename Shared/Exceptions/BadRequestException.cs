using System.Net;

namespace Shared.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message, IEnumerable<string>? errors = null)
            : base(message, (int)HttpStatusCode.BadRequest, errors) { }
    }
}