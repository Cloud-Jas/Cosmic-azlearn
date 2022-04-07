using AzureFunctions.Extensions.Middleware.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.API.Middleware
{
   public class ClaimsCheckMiddleware : ServerlessMiddleware
   {
      private readonly ILogger _logger;
      public ClaimsCheckMiddleware(ILogger logger)
      {
         _logger = logger;
      }

      public override async Task InvokeAsync(HttpContext httpContext)
      {

         var claimsPrincipal = httpContext.User;

         if (claimsPrincipal.Identity.IsAuthenticated)
         {
            await Next.InvokeAsync(httpContext);
         }
         else
         {
            httpContext.Response.ContentType = "application/json";

            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            await httpContext.Response.WriteAsync(new
            {
               StatusCode = httpContext.Response.StatusCode,
               Message = "User is not authorized to access this endpoint"
            }.ToString());
         }

      }
   }
}
