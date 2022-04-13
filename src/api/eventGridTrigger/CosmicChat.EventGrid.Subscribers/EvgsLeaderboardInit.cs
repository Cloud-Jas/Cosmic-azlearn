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
   public class EvgsLeaderboardInit
   {
      private readonly ILogger<EvgsLeaderboardInit> _logger;
      public EvgsLeaderboardInit(ILogger<EvgsLeaderboardInit> logger)
      {
         _logger = logger;
      }
      [FunctionName("LeaderboardCreate")]
      public async Task LeaderboardCreate([EventGridTrigger] EventGridEvent eventGridEvent, [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicLeaderboard", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosUser> leaderBoardCreate)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         var leaderBoard = ((JObject)(eventGridEvent.Data)).ToObject<CosmosUser>();

         await leaderBoardCreate.AddAsync(leaderBoard);         

      }
   }
}
