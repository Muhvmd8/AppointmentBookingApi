namespace Shared.Exceptions
{
    public abstract class ApiException : Exception
    {
        public virtual int StatusCode { get; }
        public IEnumerable<string>? Errors { get; }

        protected ApiException(string message, int statusCode, IEnumerable<string>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }

        protected ApiException(string message)
            : base(message)
        {
        }

        protected ApiException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}