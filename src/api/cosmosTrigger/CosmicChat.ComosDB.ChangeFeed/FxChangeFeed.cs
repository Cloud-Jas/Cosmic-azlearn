﻿using Azure.Messaging;
using CosmicChat.Shared.Models;
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
      [FunctionName("CosmosDBTrigger")]
      public async Task CosmosDBTrigger([CosmosDBTrigger(
            databaseName: "CosmicDB",
            containerName: "CosmicUsers",
            Connection = "CosmicDBIdentity",
            LeaseContainerName = "leasesCosmicUsers",
            CreateLeaseContainerIfNotExists =true)]IReadOnlyList<User> input, [EventGrid(TopicEndpointUri = "EventGridEndpoint", TopicKeySetting = "EventGridKey")] IAsyncCollector<CloudEvent> eventCollector)
      {

         if (input != null && input.Count > 0)
         {
            _logger.LogInformation("Documents modified " + input.Count);
            _logger.LogInformation("First document Id " + input[0].name);

            foreach (var doc in input)
            {
               var source = "CosmosDb.CosmicUsers";
               var type = "CosmosDb.CosmicUsers.Updated";
               var data = JsonConvert.SerializeObject(doc);

               await eventCollector.AddAsync(new CloudEvent(source, type, data)
               {
                  DataContentType = "application/cloudevents+json",
                  DataSchema = "1.0",
               });
            }
         }

      }
   }
}
