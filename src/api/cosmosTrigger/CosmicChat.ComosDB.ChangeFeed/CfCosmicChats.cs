﻿using Azure.Messaging;
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
   public class FxChangeFeed
   {
      private readonly ILogger<FxChangeFeed> _logger;

      public FxChangeFeed(ILogger<FxChangeFeed> logger)
      {
         _logger = logger;
      }
      [FunctionName("CosmicChatsTrigger")]
      public async Task CosmicChatsTrigger([CosmosDBTrigger(
            databaseName: "CosmicDB",
            containerName: "CosmicChats",
            Connection = "CosmicDBIdentity",
            LeaseContainerName = "leasesCosmicChats",
            CreateLeaseContainerIfNotExists =true)]IReadOnlyList<CosmosChat> input, [EventGrid(TopicEndpointUri = "EventGridEndpoint", TopicKeySetting = "EventGridKey")] IAsyncCollector<EventGridEvent> eventCollector)
      {

         if (input != null && input.Count > 0)
         {
            _logger.LogInformation("Documents modified " + input.Count);
            _logger.LogInformation("First document Id " + input[0].id);

            foreach (var doc in input)
            {
               var source = "CosmosDb.CosmicChats";
               var type = "CosmosDb.CosmiChats.Updated";               

               await eventCollector.AddAsync(new EventGridEvent(Guid.NewGuid().ToString("N"), source, doc, type, DateTime.UtcNow, "1.0"));               
            }
         }

      }
   }
}
