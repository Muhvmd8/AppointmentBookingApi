using System.Net;

namespace Shared.Exceptions
{
    public class ValidationException : ApiException
    {
        public ValidationException(string message, IEnumerable<string>? errors = null)
            : base(message, (int)HttpStatusCode.UnprocessableEntity, errors) { }
    }
}
