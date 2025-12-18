using System;
using Microsoft.AspNetCore.Mvc;

namespace server.Common.Filter;

public class CustomAuthorizeAttribute : TypeFilterAttribute
{
    public CustomAuthorizeAttribute() : base(typeof(AuthorizationFilter))
    {
    }
}
