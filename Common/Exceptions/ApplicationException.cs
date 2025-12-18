using System.Net;
using server.Common.Models;


namespace server.Common.Exceptions
{
	public abstract class ApplicationException : Exception
	{
		public abstract HttpStatusCode HttpStatusCode { get; }
		protected virtual string ErrorCode { get; } = string.Empty;
		protected ApplicationException(string message) : base(message)
		{
		}
		public ApiResponseModel GetErrorResponse()
		{
			return ApiResponseModel.Error(this.Message, HttpStatusCode, ErrorCode);
		}
	}
}
