using CosmicChat.Shared.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Logging;
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
            CreateLeaseContainerIfNotExists =true)]IReadOnlyList<User> input, [WebPubSub(Hub = "CosmosPark")] IAsyncCollector<WebPubSubAction> operation)
      {

         if (input != null && input.Count > 0)
         {
            _logger.LogInformation("Documents modified " + input.Count);
            _logger.LogInformation("First document Id " + input[0].name);

            await operation.AddAsync(new SendToGroupAction
            {
               Group = input[0].address.country.code + "-location",
               Data = BinaryData.FromObjectAsJson(new
               {
                  latitude = input[0].address.position.latitude,
                  longitude = input[0].address.position.longitude
               }),
               DataType = WebPubSubDataType.Json
            });
         }

      }
   }
}
