using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Middleware;
using AzureFunctions.Extensions.Middleware.Abstractions;
using CosmicChat.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmicChat.API
{
   public class FxLeaderboard
   {
      private readonly ILogger<FxLeaderboard> _logger;
      private readonly IMiddlewareBuilder _middlewareBuilder;

      public FxLeaderboard(ILogger<FxLeaderboard> log, IMiddlewareBuilder middlewareBuilder)
      {
         _logger = log;
         _middlewareBuilder = middlewareBuilder;
      }

      [FunctionName("CreateScore")]
      [OpenApiOperation(operationId: "CreateScore", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> CreateScore(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "leaderboard")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicLeaderboards", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosLeaderboard> leaderboardCreate)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
               string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

               var leaderboard = JObject.Parse(requestBody).ToObject<CosmosLeaderboard>();

               leaderboard.id = String.IsNullOrWhiteSpace(leaderboard.id) ? Guid.NewGuid().ToString("N") : leaderboard.id;               

               await leaderboardCreate.AddAsync(leaderboard);

               return new ObjectResult(leaderboard) { StatusCode = StatusCodes.Status201Created };

            }
            catch (Exception ex)
            {
               _logger.LogError($"Create chat failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }

      [FunctionName("GetCosmosLeaderBoards")]
      [OpenApiOperation(operationId: "GetCosmosLeaderBoards", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetCosmosLeaderBoards(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leaderboards")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicLeaderboards", Connection = "CosmicDBIdentity")] IEnumerable<CosmosLeaderboard> leaderboards)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
               return await Task.FromResult(new OkObjectResult(leaderboards));
            }
            catch (Exception ex)
            {
               _logger.LogError($"Get cosmos leaderboards failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }            
      
   }
}

