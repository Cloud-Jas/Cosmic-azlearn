using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Middleware;
using AzureFunctions.Extensions.Middleware.Abstractions;
using CosmicChat.Shared.Extensions;
using CosmicChat.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmicChat.API
{
   public class FxTask
   {
      private readonly ILogger<FxTask> _logger;
      private readonly IMiddlewareBuilder _middlewareBuilder;

      public FxTask(ILogger<FxTask> log, IMiddlewareBuilder middlewareBuilder)
      {
         _logger = log;
         _middlewareBuilder = middlewareBuilder;
      }

      [FunctionName("CompleteTask")]
      [OpenApiOperation(operationId: "CompleteTask", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> CompleteTask(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks/{taskId}/users/{userId}")] HttpRequest req, string taskId, string userId,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicTaskValidator", Connection = "CosmicDBIdentity", PartitionKey = "{userId}")] DocumentClient client,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUserTasks", Connection = "CosmicDBIdentity", Id = "{taskId}", PartitionKey = "{userId}")] IAsyncCollector<CosmosUserTask> userTasks)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
               string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

               var userTask = JObject.Parse(requestBody).ToObject<CosmosUserTask>();

               Uri collectionUri = UriFactory.CreateDocumentCollectionUri("CosmicDB", "CosmicTaskValidator");

               IDocumentQuery<CosmosUserValidator> query = client.CreateDocumentQuery<CosmosUserValidator>(collectionUri, new FeedOptions
               {
                  PartitionKey = new PartitionKey(userId)
               })
                .Where(p => p._ts < DateTimeOffset.Now.ToUnixTimeSeconds() && p._ts > DateTimeExtensions.GetPastHourTimestamp())
                .AsDocumentQuery();

               while (query.HasMoreResults)
               {
                  foreach (CosmosUserValidator validatorResult in await query.ExecuteNextAsync())
                  {
                     Uri usersCollectionUri = UriFactory.CreateDocumentCollectionUri("CosmicDB", "CosmicUsers");

                     IDocumentQuery<CosmosUser> userQuery = client.CreateDocumentQuery<CosmosUser>(usersCollectionUri, new FeedOptions
                     {
                        PartitionKey = new PartitionKey(validatorResult.toUserId)
                     })
                      .AsDocumentQuery();

                     while (query.HasMoreResults)
                     {
                        foreach (CosmosUser userResult in await query.ExecuteNextAsync())
                        {
                           if (userTask.taskDetail.country.subDivision.Equals(userResult.address.country.subDivision) && userTask.taskDetail.country.name.Equals(userResult.address.country.name))
                           {
                              userTask.isCompleted = true;
                              await userTasks.AddAsync(userTask);
                              return new OkResult();
                           }
                        }

                     }
                  }
               }

               return new NoContentResult();

            }
            catch (Exception ex)
            {
               _logger.LogError($"Complete tasks failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }

      [FunctionName("GetAllTasksByUserId")]
      [OpenApiOperation(operationId: "GetAllTasksByUserId", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetAllTasksByUserId(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/users/{userId}")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUserTasks", Connection = "CosmicDBIdentity", PartitionKey = "{userId}")] IEnumerable<CosmosUserTask> userTasks)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
               return await Task.FromResult(new OkObjectResult(userTasks));
            }
            catch (Exception ex)
            {
               _logger.LogError($"Get cosmos user tasks failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }

   }
}

