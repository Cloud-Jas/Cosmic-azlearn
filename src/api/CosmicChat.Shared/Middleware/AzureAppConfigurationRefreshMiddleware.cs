using AzureFunctions.Extensions.Middleware.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Middleware
{
   public class AzureAppConfigurationRefreshMiddleware : ServerlessMiddleware
   {
      private readonly ILogger _logger;
      public IEnumerable<IConfigurationRefresher> Refreshers { get; }
      public AzureAppConfigurationRefreshMiddleware(ILogger logger,IConfigurationRefresherProvider refresherProvider)
      {
         _logger = logger;
         Refreshers= refresherProvider.Refreshers;
      }
      public override async Task InvokeAsync(HttpContext httpContext)
      {
         foreach(var refresher in Refreshers)
         {
            _ = refresher.TryRefreshAsync();
         }
         await Next.InvokeAsync(httpContext);
      }
   }
}
