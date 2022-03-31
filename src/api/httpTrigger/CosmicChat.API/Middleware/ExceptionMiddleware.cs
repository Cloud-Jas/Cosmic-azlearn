using AzureFunctions.Extensions.Middleware.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CosmicChat.API.Middleware
{
   public class ExceptionMiddleware : ServerlessMiddleware
   {
      private readonly ILogger _logger;
      public ExceptionMiddleware(ILogger logger)
      {
         _logger = logger;
      }

      public override async Task InvokeAsync(HttpContext httpContext)
      {
         try
         {
            await Next.InvokeAsync(httpContext);
         }
         catch(Exception ex)
         {
            httpContext.Response.ContentType = "application/json";

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await httpContext.Response.WriteAsync(new
            {
               StatusCode = httpContext.Response.StatusCode,
               Message = ex.Message
            }.ToString());
         }
      }
   }
}
