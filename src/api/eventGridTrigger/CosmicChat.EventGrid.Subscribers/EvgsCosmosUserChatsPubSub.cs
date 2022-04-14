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
   public class EvgsCosmosUserChatsPubSub
   {
      private readonly ILogger<EvgsCosmosUserChatsPubSub> _logger;
      public EvgsCosmosUserChatsPubSub(ILogger<EvgsCosmosUserChatsPubSub> logger)
      {
         _logger = logger;
      }
      [FunctionName("CosmosUserChatsPubSub")]
      public async Task CosmosChat([EventGridTrigger] EventGridEvent eventGridEvent, [WebPubSub(Hub = "CosmosPark")] IAsyncCollector<WebPubSubAction> operation)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         if (eventGridEvent.EventType.Equals("CosmosDb.CosmiUserChats.Updated"))
         {
            var userChat = ((JObject)(eventGridEvent.Data)).ToObject<CosmosUserChat>();

            await operation.AddAsync(new SendToGroupAction
            {
               Group = "chats-"+userChat.userId,
               Data = BinaryData.FromObjectAsJson(new
               {
                  chatName=userChat.chatName,
                  toUserId=userChat.toUserId,
                  chatId=userChat.chatId,
                  lastMessage=userChat.lastMessage,
                  lastMessageTimestamp=userChat.lastMessageTimestamp,
                  type="ChatsBarUpdatedEvent"
               }),
               DataType = WebPubSubDataType.Json
            });
         }

      }
   }
}
