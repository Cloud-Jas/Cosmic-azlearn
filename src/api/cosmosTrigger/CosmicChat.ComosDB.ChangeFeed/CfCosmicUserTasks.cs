using Azure.Messaging;
using CosmicChat.Shared.Models;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmicChat.CosmosDB.ChangeFeed
{
   public class CfCosmicUserTasks
   {
      private readonly ILogger<CfCosmicUserTasks> _logger;

      public CfCosmicUserTasks(ILogger<CfCosmicUserTasks> logger)
      {
         _logger = logger;
      }
      [FunctionName("CosmicUserTasksTrigger")]
      public async Task CosmicUserTasksTrigger([CosmosDBTrigger(
            databaseName: "CosmicDB",
            containerName: "CosmicUserTasks",
            Connection = "CosmicDBIdentity",
            LeaseContainerName = "leasesCosmicUserTasks",
            CreateLeaseContainerIfNotExists =true)]IReadOnlyList<CosmosUserTask> input, [EventGrid(TopicEndpointUri = "EventGridEndpoint", TopicKeySetting = "EventGridKey")] IAsyncCollector<EventGridEvent> eventCollector)
      {

         if (input != null && input.Count > 0)
         {
            _logger.LogInformation("Documents modified " + input.Count);
            _logger.LogInformation("First document Id " + input[0].id);

            foreach (var doc in input)
            {
               var source = "CosmosDb.CosmicUserTasks";
               var type = "CosmosDb.CosmicUserTasks.Updated";

               if (doc.isCompleted)
               {
                  await eventCollector.AddAsync(new EventGridEvent(Guid.NewGuid().ToString("N"), source, doc, type, DateTime.UtcNow, "1.0"));
               }
            }
         }

      }
   }
}
