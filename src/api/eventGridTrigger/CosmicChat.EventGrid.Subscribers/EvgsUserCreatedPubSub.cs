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
   public class EvgsUserCreatedPubSub
   {
      private readonly ILogger<EvgsUserCreatedPubSub> _logger;
      public EvgsUserCreatedPubSub(ILogger<EvgsUserCreatedPubSub> logger)
      {
         _logger = logger;
      }
      [FunctionName("UserCreated")]
      public async Task UserCreated([EventGridTrigger] EventGridEvent eventGridEvent, [WebPubSub(Hub = "CosmosPark")] IAsyncCollector<WebPubSubAction> operation)
      {
         _logger.LogInformation(eventGridEvent.Data.ToString());

         if (eventGridEvent.EventType.Equals("CosmosDb.CosmicUsers.Created"))
         {
            var user = ((JObject)(eventGridEvent.Data)).ToObject<User>();

            await operation.AddAsync(new SendToGroupAction
            {
               Group = "global-location",
               Data = BinaryData.FromObjectAsJson(new
               {
                  latitude = user.address.position.latitude,
                  longitude = user.address.position.longitude,
                  userId = user.id,
                  userName = user.name,
                  city = user.address.country.secondarySubDivision,
                  state = user.address.country.subDivision,
                  type="UserCreatedEvent"
               }),
               DataType = WebPubSubDataType.Json
            });
         }

      }
   }
}
