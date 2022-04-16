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
      public async Task LeaderboardCreate([EventGridTrigger] EventGridEvent eventGridEvent, 
         [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicLeaderboards", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosLeaderboard> leaderBoardCreate)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         try
         {

            if (eventGridEvent.EventType.Equals("CosmosDb.CosmicUsers.Created"))
            {
               _logger.LogInformation("CosmicUsers created event fired");

               var Cosmicuser = ((JObject)(eventGridEvent.Data)).ToObject<CosmosUser>();

               _logger.LogInformation(JsonConvert.SerializeObject(Cosmicuser));                              

               await leaderBoardCreate.AddAsync(new CosmosLeaderboard
               {
                  id = Guid.NewGuid().ToString("N"),
                  score = 0,
                  task =
               {
                  id=null
               },
                  user =
               {
                  id=Cosmicuser?.id,
                  name=Cosmicuser?.name
               }
               });

               _logger.LogInformation("leaderboard updated");
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex.Message+ex.StackTrace.ToString());
         }
         
      }
   }
}
