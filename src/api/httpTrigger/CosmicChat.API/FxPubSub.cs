using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Messaging.WebPubSub;
using CosmicChat.Shared.Extensions;
using CosmicChat.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace CosmicChat.API
{
   public class FxPubSub
   {
      private readonly ILogger<FxPubSub> _logger;
      private readonly WebPubSubServiceClient _client;
      private readonly IOptions<WebPubSubOptions> _webPubSub;

      public FxPubSub(ILogger<FxPubSub> log,IOptions<WebPubSubOptions> webPubSub)
      {
         _logger = log;
         _webPubSub = webPubSub;
         _client = new WebPubSubServiceClient(new Uri(_webPubSub.Value.Endpoint),_webPubSub.Value.Hub,new AzureKeyCredential(_webPubSub.Value.AccessKey));
      }

      /// <summary>
      /// Get token
      /// For security consideration by default client cannot publish or subscribe to 
      /// a group by itself. We need to generate code to give client roles 
      /// Available roles:
      /// [SendToGroup,JoinLeaveGroup]
      /// </summary>
      /// <param name="req"></param>
      /// <param name="claimsPrincipal"></param>
      /// <returns></returns>
      [FunctionName("GetConnectionDetails")]
      [ProducesResponseType(200)]
      [ProducesResponseType(401)]
      [ProducesResponseType(404)]
      public async Task<IActionResult> GetConnectionDetails(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pubsub/connection")] HttpRequest req, ILogger log)
      {

         log.LogInformation("Get WebPubSub connection details triggered!");

         var user = req.ParseUser();

         try
         {
            var response = await _client.GetClientAccessUriAsync(DateTimeOffset.UtcNow.AddHours(Convert.ToDouble(_webPubSub.Value.ExpireAt)),user.id,_webPubSub.Value.Roles);

            log.LogInformation("Server URI fetched successfully");

            return new OkObjectResult(new { url = response });
         }
         catch (Exception ex)
         {
            log.LogError($"WebPubSub GetConnectionDetails failed {ex.Message}", ex);

            return new NotFoundResult();
         }
      }
   }
}

