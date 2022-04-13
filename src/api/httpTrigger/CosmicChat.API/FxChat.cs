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
   public class FxChat
   {
      private readonly ILogger<FxChat> _logger;
      private readonly IMiddlewareBuilder _middlewareBuilder;

      public FxChat(ILogger<FxChat> log, IMiddlewareBuilder middlewareBuilder)
      {
         _logger = log;
         _middlewareBuilder = middlewareBuilder;
      }

      [FunctionName("CreateChat")]
      [OpenApiOperation(operationId: "CreateChat", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> CreateChat(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicChats", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosChat> chatsCreate)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
               string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

               var chat = JObject.Parse(requestBody).ToObject<CosmosChat>();

               chat.id = String.IsNullOrWhiteSpace(chat.id) ? Guid.NewGuid().ToString("N") : chat.id;

               chat.message.timestamp = Convert.ToInt64(DateTimeOffset.Now.ToUnixTimeSeconds());

               await chatsCreate.AddAsync(chat);

               return new ObjectResult(chat) { StatusCode = StatusCodes.Status201Created };

            }
            catch (Exception ex)
            {
               _logger.LogError($"Create chat failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }

      [FunctionName("GetAllChatsByUserId")]
      [OpenApiOperation(operationId: "GetAllChatsByUserId", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetAllChatsByUserId(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chats/user/{userId}")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUserChats", Connection = "CosmicDBIdentity", PartitionKey ="{userId}")] IEnumerable<CosmosUserChat> chats)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
               return await Task.FromResult(new OkObjectResult(chats));
            }
            catch (Exception ex)
            {
               _logger.LogError($"Get all chats by user id failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }            
      [FunctionName("GetAllMessagesByChatId")]
      [OpenApiOperation(operationId: "GetAllMessagesByChatId", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetAllMessagesByChatId(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chats/{chatId}")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicChats", Connection = "CosmicDBIdentity", PartitionKey = "{chatId}")] IEnumerable<CosmosChat> chats)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
               return await Task.FromResult(new OkObjectResult(chats));
            }
            catch (Exception ex)
            {
               _logger.LogError($"Get all messages by chat id failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }
   }
}

