using System.Net;

namespace server.Common.Exceptions
{
	public class ValidationException : ApplicationException
	{
		protected override string ErrorCode { get; }
		public ValidationException(string message) : base(message)
		{
		}

		public override HttpStatusCode HttpStatusCode => HttpStatusCode.BadRequest;
	}
}
