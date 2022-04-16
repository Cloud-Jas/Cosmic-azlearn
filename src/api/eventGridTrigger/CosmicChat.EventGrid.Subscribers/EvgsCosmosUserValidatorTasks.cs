// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CosmicChat.Shared.Models;
using System.Collections.Generic;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json.Linq;

namespace CosmicChat.EventGrid.Subscribers
{
   public class EvgsCosmosUserValidatorTasks
   {
      private readonly ILogger<EvgsCosmosUserValidatorTasks> _logger;
      public EvgsCosmosUserValidatorTasks(ILogger<EvgsCosmosUserValidatorTasks> logger)
      {
         _logger = logger;
      }
      [FunctionName("CosmicUserValidatorTasks")]
      public async Task CosmicUserValidatorTasks([EventGridTrigger] EventGridEvent eventGridEvent,
         [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicTaskValidator", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosUserValidator> cosmosUserValidatorCreate)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         if (eventGridEvent.EventType.Equals("CosmosDb.CosmiChats.Updated"))
         {
            var cosmosChat = ((JObject)(eventGridEvent.Data)).ToObject<CosmosChat>();

            string toUserId = string.Empty;

            if (string.Equals(cosmosChat.message.content, "Hi-CosmosPark", StringComparison.OrdinalIgnoreCase))
            {
               foreach (var userDetail in cosmosChat.userDetails)
               {
                  if (userDetail.id == cosmosChat.message.senderId)
                     toUserId = userDetail.toUser.id;
               }
               var validator = new CosmosUserValidator
               {
                  id = Guid.NewGuid().ToString("N"),
                  userId = cosmosChat.message.senderId,
                  toUserId = toUserId
               };
               await cosmosUserValidatorCreate.AddAsync(validator);
            }
         }

      }
   }
}
