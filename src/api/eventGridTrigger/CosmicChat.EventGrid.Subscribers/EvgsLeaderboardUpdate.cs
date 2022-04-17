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
   public class EvgsLeaderboardUpdate
   {
      private readonly ILogger<EvgsLeaderboardUpdate> _logger;
      public EvgsLeaderboardUpdate(ILogger<EvgsLeaderboardUpdate> logger)
      {
         _logger = logger;
      }
      [FunctionName("LeaderboardUpdate")]
      public async Task LeaderboardUpdate([EventGridTrigger] EventGridEvent eventGridEvent,
         [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicLeaderboards", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosLeaderboard> leaderBoardCreate)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         try
         {

            if (eventGridEvent.EventType.Equals("CosmosDb.CosmicUserTasks.Updated"))
            {
               _logger.LogInformation("CosmicUserTaskss updated event fired");

               var CosmicuserTask = ((JObject)(eventGridEvent.Data)).ToObject<CosmosUserTask>();

               _logger.LogInformation(JsonConvert.SerializeObject(CosmicuserTask));

               await leaderBoardCreate.AddAsync(new CosmosLeaderboard
               {
                  id = Guid.NewGuid().ToString("N"),
                  score = 2,
                  task = new LeaderboardTask
                  {
                     id = CosmicuserTask.id
                  },
                  user = new User
                  {
                     id = CosmicuserTask.userId,
                     name = CosmicuserTask.userName
                  }
               });

               _logger.LogInformation("leaderboard updated");
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex.Message + ex.StackTrace.ToString());
         }

      }
   }
}
