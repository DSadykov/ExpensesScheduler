using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Http;


namespace ExpensesScheduler.Authorization.Extensions;

public static class HttpContextExtensions
{
    public static Guid GetUserID(this HttpContext httpContext)
    {
        return (Guid)httpContext.Items["UserId"];
    }
}
