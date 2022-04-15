using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmicChat.Shared.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CosmicChat.GlobalTimer.Trigger
{
   public class TimerCosmos
   {
      private readonly ILogger _logger;
      public TimerCosmos(ILogger<TimerCosmos> logger)
      {
         _logger = logger;
      }
      [FunctionName("TimerCosmos")]
      public async Task CreateGlobalTimer([TimerTrigger("0 0 15/30 * *")] TimerInfo myTimer,
         [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicGlobalTimer", Connection = "CosmicDBIdentity", SqlQuery = "SELECT TOP 1 * FROM c ORDER BY c._ts DESC")] IEnumerable<CosmosGlobalTimer> globalTimers,
         [CosmosDB(databaseName: "CosmicDB", containerName: "CosmicGlobalTimer", Connection = "CosmicDBIdentity")] IAsyncCollector<CosmosGlobalTimer> globalTimerCreate)
      {
         _logger.LogInformation(DateTime.Now.ToString());

         foreach (var timer in globalTimers)
         {
            _logger.LogInformation(JsonConvert.SerializeObject(timer));

            var globalTimer = new CosmosGlobalTimer
            {
               id = Guid.NewGuid().ToString("N"),
               timestamp = new Timestamp
               {
                  startTimestampInSeconds = timer.timestamp._30DaysTimestampInSeconds,
                  _7DaysTimestampInSeconds = timer.timestamp._30DaysTimestampInSeconds + (3600 * 24 * 7),
                  _30DaysTimestampInSeconds = timer.timestamp._30DaysTimestampInSeconds + (3600*24*30)
               }
            };

            await globalTimerCreate.AddAsync(globalTimer);
         }

      }
   }
}
