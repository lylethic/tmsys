using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace server.Common.Exceptions
{
    public class NonAuthorizeException(string message = "Not Authorized") : ApplicationException(message)
    {
        public override HttpStatusCode HttpStatusCode => HttpStatusCode.Unauthorized;
    }
}
