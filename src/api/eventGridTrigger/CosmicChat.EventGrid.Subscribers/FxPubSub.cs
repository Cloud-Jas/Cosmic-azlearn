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

namespace CosmicChat.EventGrid.Subscribers
{
   public class FxPubSub
   {
      private readonly ILogger<FxPubSub> _logger;
      public FxPubSub(ILogger<FxPubSub> logger)
      {
         _logger = logger;
      }
      [FunctionName("UserOnline")]
      public async Task UserJoin([EventGridTrigger] CloudEvent cloudEvent, [WebPubSub(Hub = "CosmosPark")] IAsyncCollector<WebPubSubAction> operation)
      {
         _logger.LogInformation(cloudEvent.Data.ToString());

         var user = JsonConvert.DeserializeObject<User>(cloudEvent.Data.ToString());

         await operation.AddAsync(new SendToGroupAction
         {
            Group = user.address.country.code + "-location",
            Data = BinaryData.FromObjectAsJson(new
            {
               latitude = user.address.position.latitude,
               longitude = user.address.position.longitude,
               userId = user.id,
               userName = user.name,
               city = user.address.country.secondarySubDivision,
               state = user.address.country.subDivision
            }),
            DataType = WebPubSubDataType.Json
         });

      }
   }
}
