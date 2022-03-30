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

namespace azlearn.cosmic.API
{
    public class FxAzureMap
    {
        private readonly ILogger<FxAzureMap> _logger;

        private static readonly string[] allowd = { "https://wonderful-river-07ec3e210.1.azurestaticapps.net/",
                                                    "http://localhost"};

        public FxAzureMap(ILogger<FxAzureMap> log)
        {
            _logger = log;
        }

        [FunctionName("FxAzureMap")]
        [OpenApiOperation(operationId: "GetToken", tags: new[] { "name" })]        
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> GetToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azureMapsToken")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            /*string referer = req.Headers["Referer"];
            if (string.IsNullOrEmpty(referer))
               return new UnauthorizedResult();

            string result = Array.Find(allowd, site => referer.StartsWith(site, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(result))
               return new UnauthorizedResult();*/

            var tokenCredential = new DefaultAzureCredential();
            var accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(new[] { "https://atlas.microsoft.com/.default" })
            );

            return new OkObjectResult(accessToken.Token);         
        }
    }
}

