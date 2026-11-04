namespace Shared.Exceptions;

public class ExternalServiceException : ApiException
{
    public ExternalServiceException(string message) : base(message, 500) { }

    public ExternalServiceException(string message, Exception inner)
        : base(message, inner) { }
}
