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
   public class FxPubSub
   {
      private readonly ILogger<FxPubSub> _logger;
      public FxPubSub(ILogger<FxPubSub> logger)
      {
         _logger = logger;
      }
      [FunctionName("CosmosChat")]
      public async Task CosmosChat([EventGridTrigger] EventGridEvent eventGridEvent, [WebPubSub(Hub = "CosmosPark")] IAsyncCollector<WebPubSubAction> operation)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         if (eventGridEvent.EventType.Equals("CosmosDb.CosmiChats.Updated"))
         {
            var chatResponse = ((JObject)(eventGridEvent.Data)).ToObject<CosmosChat>();

            await operation.AddAsync(new SendToGroupAction
            {
               Group = chatResponse.chat.id,
               Data = BinaryData.FromObjectAsJson(new
               {
                  chatId = chatResponse.chat.id,
                  senderId = chatResponse.message.senderId,
                  senderMessage = chatResponse.message.content,
                  timestamp = chatResponse.message.timestamp,
                  messageId = chatResponse.id,
                  chatUsers = chatResponse.userIds
               }),
               DataType = WebPubSubDataType.Json
            });
         }

      }
   }
}
