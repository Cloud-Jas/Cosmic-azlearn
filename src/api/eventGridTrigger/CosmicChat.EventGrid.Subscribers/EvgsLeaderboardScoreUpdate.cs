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
   public class EvgsLeaderboardScoreUpdate
   {
      private readonly ILogger<EvgsLeaderboardScoreUpdate> _logger;
      public EvgsLeaderboardScoreUpdate(ILogger<EvgsLeaderboardScoreUpdate> logger)
      {
         _logger = logger;
      }
      [FunctionName("ScoreUpdated")]
      public async Task ScoreUpdated([EventGridTrigger] EventGridEvent eventGridEvent, [WebPubSub(Hub = "CosmosPark")] IAsyncCollector<WebPubSubAction> operation)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         if (eventGridEvent.EventType.Equals("CosmosDb.CosmicLeaderboards.Updated"))
         {
            var leaderboard = ((JObject)(eventGridEvent.Data)).ToObject<CosmosLeaderboard>();

            await operation.AddAsync(new SendToGroupAction
            {
               Group = "global-score",
               Data = BinaryData.FromObjectAsJson(new
               {                  
                  userId = leaderboard.user.id,
                  userName = leaderboard.user.name,
                  score= leaderboard.score,
                  type="ScoreUpdatedEvent"
               }),
               DataType = WebPubSubDataType.Json
            });
         }

      }
   }
}
