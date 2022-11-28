using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Azure.Core;
using Azure.Identity;
using System;
using AzureFunctions.Extensions.Middleware.Abstractions;
using AzureFunctions.Extensions.Middleware;
using System.Net.Http;
using Microsoft.Extensions.Options;
using CosmicChat.Shared.Models;

namespace azlearn.cosmic.API
{
   public class FxAzureMap
   {
      private readonly ILogger<FxAzureMap> _logger;
      private readonly IHttpMiddlewareBuilder _middlewareBuilder;
      private readonly IOptions<AzureMapConfiguration> _azMapConfiguration;

      private static readonly string[] allowd = { "https://cosmic-azpark.azurewebsites.net/",
                                                    "http://localhost"};

      public FxAzureMap(ILogger<FxAzureMap> log, IHttpMiddlewareBuilder middlewareBuilder, IOptions<AzureMapConfiguration> azMapConfiguration)
      {
         _logger = log;
         _middlewareBuilder = middlewareBuilder;
         _azMapConfiguration = azMapConfiguration;
      }

      [FunctionName("GetToken")]
      [OpenApiOperation(operationId: "GetToken", tags: new[] { "name" })]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetToken(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azMap/token")] HttpRequest req)
      {
         return await _middlewareBuilder.ExecuteAsync(new HttpMiddleware(async (httpContext) =>
         {
            try
            {
               _logger.LogInformation("C# HTTP trigger function processed a request.");

               string referer = req.Headers["Referer"];
               if (string.IsNullOrEmpty(referer))
                  return new UnauthorizedResult();

               string result = Array.Find(allowd, site => referer.StartsWith(site, StringComparison.OrdinalIgnoreCase));
               if (string.IsNullOrEmpty(result))
                  return new UnauthorizedResult();

               var tokenCredential = new DefaultAzureCredential();
               var accessToken = await tokenCredential.GetTokenAsync(
                   new TokenRequestContext(new[] { "https://atlas.microsoft.com/.default" })
               );

               req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");


               return new OkObjectResult(accessToken.Token);
            }
            catch (Exception ex)
            {
               _logger.LogError(ex.Message, ex);
               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
         }));
      }
      [FunctionName("LookupAddress")]
      [OpenApiOperation(operationId: "LookupAddress", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> LookupAddress(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azMap/addressLookup/{latitude}/{longitude}")] HttpRequest req, string latitude, string longitude)
      {
         return await _middlewareBuilder.ExecuteAsync(new HttpMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
               HttpClient client = new HttpClient();

               var response = await client.GetAsync("https://atlas.microsoft.com/search/address/reverse/json?api-version=1.0&query=" + latitude
                  + "," + longitude + "&subscription-key=" + _azMapConfiguration?.Value?.Key?.ToString());

               var content = await response.Content.ReadAsStringAsync();

               return new OkObjectResult(content);
            }
            catch (Exception ex)
            {
               _logger.LogError($"Lookup address failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }
   }
}

