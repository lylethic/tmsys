﻿using System.Net;

namespace server.Common.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        private static readonly string _defaultErrorMsg = "Not Found";
        public override HttpStatusCode HttpStatusCode => HttpStatusCode.OK;
        public NotFoundException() : base(_defaultErrorMsg)
        {
            ErrorCode = "NOT_FOUND";
        }

        public NotFoundException(string message, string errorCode = "NOT_FOUND") : base(message)
        {
            ErrorCode = errorCode;
        }

        protected override string ErrorCode { get; }
    }
}
