using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ExpensesScheduler.Authorization.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Items["UserId"] is not Guid)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized"
            });
        }
    }
}
