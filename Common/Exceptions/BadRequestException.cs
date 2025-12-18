using System.Net;

namespace server.Common.Exceptions
{
    public class BadRequestException : ApplicationException
    {
        private static readonly string _defaultErrorMsg = "Bad Request";

        public override HttpStatusCode HttpStatusCode => HttpStatusCode.BadRequest;

        public BadRequestException() : base(_defaultErrorMsg)
        {
            ErrorCode = "BAD_REQUEST";
        }

        public BadRequestException(string message, string errorCode = "BAD_REQUEST") : base(message)
        {
            ErrorCode = errorCode;
        }

        protected override string ErrorCode { get; }
    }
}
