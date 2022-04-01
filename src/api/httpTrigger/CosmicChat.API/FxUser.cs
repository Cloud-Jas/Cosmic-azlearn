using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace CosmicChat.API
{
   public class FxUser
   {
      private readonly ILogger<FxUser> _logger;

      public FxUser(ILogger<FxUser> log)
      {
         _logger = log;
      }

      [FunctionName("FxUser")]
      [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetUsers(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req,          
          [CosmosDB(databaseName:"CosmicDB",containerName:"CosmicUsers",Connection ="CosmicDBIdentity")] IEnumerable<dynamic> users)
      {
         _logger.LogInformation("C# HTTP trigger function processed a request.");

       //  if (claimsPrincipal.Identity.IsAuthenticated)
        // {
            try
            {
               //var response = await _sessionRepository.GetAllUsers();

               return new OkObjectResult(users);
            }
            catch (Exception ex)
            {
               _logger.LogError($"Get all Users failed : { ex }");

               return new NotFoundResult();
            }
        // }
        // else
        // {
        //    _logger.LogError("UnAuthorized access to Get all sessions trigger");

         //   return new UnauthorizedResult();
         //}
      }
   }
}

