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
using Microsoft.Azure.Cosmos;
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

      [FunctionName("CreateTask")]
      [OpenApiOperation(operationId: "CreateTask", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> CreateTask(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "task")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUserTasks", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosUserTask> tasksCreate)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
               string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

               var userTask = JObject.Parse(requestBody).ToObject<CosmosUserTask>();

               userTask.id = String.IsNullOrWhiteSpace(userTask.id) ? Guid.NewGuid().ToString("N") : userTask.id;

               await tasksCreate.AddAsync(userTask);

               return new ObjectResult(userTask) { StatusCode = StatusCodes.Status201Created };

            }
            catch (Exception ex)
            {
               _logger.LogError($"Create task failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }

      [FunctionName("CompleteTask")]
      [OpenApiOperation(operationId: "CompleteTask", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> CompleteTask(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks/{taskId}/users/{userId}")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicTaskValidator", Connection = "CosmicDBIdentity", PartitionKey = "{userId}")] CosmosClient client,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUserTasks", Connection = "CosmicDBIdentity", Id = "{taskId}", PartitionKey = "{userId}")] IAsyncCollector<CosmosUserTask> userTasks)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
               string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

               var userTask = JObject.Parse(requestBody).ToObject<CosmosUserTask>();

               Container container = client.GetDatabase("CosmicDB").GetContainer("CosmicTaskValidator");

               QueryDefinition queryDefinition = new QueryDefinition("Select * from p where  p._ts < @UnixTimeSeconds and p._ts > @PastHourTimestamp")
               .WithParameter("@UnixTimeSeconds", DateTimeOffset.Now.ToUnixTimeSeconds())
               .WithParameter("@PastHourTimestamp", DateTimeExtensions.GetPastHourTimestamp());

               using (FeedIterator<CosmosUserValidator> resultSet = container.GetItemQueryIterator<CosmosUserValidator>(queryDefinition))
               {
                  while (resultSet.HasMoreResults)
                  {
                     FeedResponse<CosmosUserValidator> validatorResultSet = await resultSet.ReadNextAsync();

                     CosmosUserValidator validatorResult = validatorResultSet.First();

                     Container userContainer = client.GetDatabase("CosmicDB").GetContainer("CosmicUsers");

                     QueryDefinition userQueryDefinition = new QueryDefinition("Select * from c where c.id=@userId")
               .WithParameter("@userId", validatorResult.toUserId);

                     using (FeedIterator <CosmosUser> userResultSet = userContainer.GetItemQueryIterator<CosmosUser>(userQueryDefinition))
                     {
                        while (userResultSet.HasMoreResults)
                        {
                           FeedResponse<CosmosUser> userResponseResultSet = await userResultSet.ReadNextAsync();

                           CosmosUser userResponse = userResponseResultSet.First();

                           if (userTask.taskDetail.country.subDivision.Equals(userResponse.address.country.subDivision, StringComparison.OrdinalIgnoreCase) && userTask.taskDetail.country.name.Equals(userResponse.address.country.name, StringComparison.OrdinalIgnoreCase))
                           {
                              userTask.isCompleted = true;
                              await userTasks.AddAsync(userTask);
                              return new OkObjectResult(new
                              {
                                 Success=true
                              });
                           }
                        }

                     }
                  }
               }

               return new OkObjectResult(new
               {
                  Success= false
               });

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

