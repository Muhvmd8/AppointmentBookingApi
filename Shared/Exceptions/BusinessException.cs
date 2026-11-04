using System.Net;

namespace Shared.Exceptions;

public class BusinessException : ApiException
{
    public BusinessException(string message, IEnumerable<string>? errors = null)
          : base(message, (int)HttpStatusCode.BadRequest, errors) { }
}
