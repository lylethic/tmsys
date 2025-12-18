using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace server.Common.Exceptions
{
    public class NonAuthenticateException(string message = "Invalid Username or Password") : ApplicationException(message)
    {
        public override HttpStatusCode HttpStatusCode => HttpStatusCode.Forbidden;
        protected override string ErrorCode { get; } = "";
    }
}
