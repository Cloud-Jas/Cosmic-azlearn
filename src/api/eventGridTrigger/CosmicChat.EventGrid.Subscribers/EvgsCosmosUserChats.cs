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
   public class EvgsCosmosUserChats
   {
      private readonly ILogger<EvgsCosmosUserChats> _logger;
      public EvgsCosmosUserChats(ILogger<EvgsCosmosUserChats> logger)
      {
         _logger = logger;
      }
      [FunctionName("CosmicUserChatsCreate")]
      public async Task CosmicUserChatsCreate([EventGridTrigger] EventGridEvent eventGridEvent, [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicUserChats", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosUserChat> cosmosUserChatCreate)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         var cosmosChat = ((JObject)(eventGridEvent.Data)).ToObject<CosmosChat>();

         //interchange username logic : TODO revisit for groups implementation

         int i = 0;

         foreach (var user in cosmosChat.userDetails)
         {
            if (i == 1) i = 0;
            var cosmosUserChat = new CosmosUserChat
            {
               chatId = cosmosChat.chat.id,
               lastMessage = cosmosChat.message.content,
               senderId = cosmosChat.message.senderId,
               chatName = cosmosChat.userDetails[i++].name,
               id = cosmosChat.chat.id,
               userId = user.id,
               lastMessageTimestamp= Convert.ToInt64(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };
            await cosmosUserChatCreate.AddAsync(cosmosUserChat);            
         }

      }
   }
}
