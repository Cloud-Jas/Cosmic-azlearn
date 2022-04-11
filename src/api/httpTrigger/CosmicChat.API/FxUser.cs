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
   public class FxUser
   {
      private readonly ILogger<FxUser> _logger;
      private readonly IMiddlewareBuilder _middlewareBuilder;

      public FxUser(ILogger<FxUser> log, IMiddlewareBuilder middlewareBuilder)
      {
         _logger = log;
         _middlewareBuilder = middlewareBuilder;
      }

      [FunctionName("CreateUser")]
      [OpenApiOperation(operationId: "CreateUser", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> CreateUser(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUsers", Connection = "CosmicDBIdentity")] IAsyncCollector<User> usersCreate)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
               string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

               var user = JObject.Parse(requestBody).ToObject<User>();

               await usersCreate.AddAsync(user);

               return new ObjectResult(user) { StatusCode = StatusCodes.Status201Created };

            }
            catch (Exception ex)
            {
               _logger.LogError($"Create user failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }

      [FunctionName("GetUserById")]
      [OpenApiOperation(operationId: "GetUserById", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetUserById(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{userId}")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUsers", Connection = "CosmicDBIdentity", Id = "{userId}")] User user)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
               return await Task.FromResult(new OkObjectResult(user));
            }
            catch (Exception ex)
            {
               _logger.LogError($"Get user by id failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }      
      [FunctionName("GetAllUsers")]
      [OpenApiOperation(operationId: "GetAllUsers", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetAllUsers(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUsers", Connection = "CosmicDBIdentity")] IEnumerable<User> users)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
               return await Task.FromResult(new OkObjectResult(users));
            }
            catch (Exception ex)
            {
               _logger.LogError($"Get all Users failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

         }));
      }
      [FunctionName("GetAllUsersByCountryCode")]
      [OpenApiOperation(operationId: "GetAllUsersByCountryCode", tags: new[] { "name" })]
      [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
      [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
      public async Task<IActionResult> GetAllUsersByCountryCode(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{countryCode}")] HttpRequest req,
          [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUsers", Connection = "CosmicDBIdentity", PartitionKey = "{countryCode}")] IEnumerable<User> users)
      {
         return await _middlewareBuilder.ExecuteAsync(new FunctionsMiddleware(async (httpContext) =>
         {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
               return await Task.FromResult(new OkObjectResult(users));
            }
            catch (Exception ex)
            {
               _logger.LogError($"Get all Users by country code failed : { ex }");

               return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


         }));
      }
   }
}

