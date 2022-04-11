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
   public class CfCosmicUsers
   {
      private readonly ILogger<CfCosmicUsers> _logger;

      public CfCosmicUsers(ILogger<CfCosmicUsers> logger)
      {
         _logger = logger;
      }
      [FunctionName("CosmicUsersTrigger")]
      public async Task CosmicUsersTrigger([CosmosDBTrigger(
            databaseName: "CosmicDB",
            containerName: "CosmicUsers",
            Connection = "CosmicDBIdentity",
            LeaseContainerName = "leasesCosmicUsers",
            CreateLeaseContainerIfNotExists =true)]IReadOnlyList<User> input, [EventGrid(TopicEndpointUri = "EventGridEndpoint", TopicKeySetting = "EventGridKey")] IAsyncCollector<EventGridEvent> eventCollector)
      {

         if (input != null && input.Count > 0)
         {
            _logger.LogInformation("Documents modified " + input.Count);
            _logger.LogInformation("First document Id " + input[0].name);

            foreach (var doc in input)
            {
               var source = "CosmosDb.CosmicUsers";
               var type = "CosmosDb.CosmicUsers.Created";               

               await eventCollector.AddAsync(new EventGridEvent(Guid.NewGuid().ToString("N"), source, doc, type, DateTime.UtcNow, "1.0"));               
            }
         }

      }
   }
}
