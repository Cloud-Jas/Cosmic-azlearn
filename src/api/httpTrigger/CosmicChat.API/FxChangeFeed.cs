using CosmicChat.API.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.API
{
   public class FxChangeFeed
   {
      private readonly ILogger _logger;
      public FxChangeFeed(ILogger logger)
      {
         _logger = logger;
      }
      [FunctionName("CosmosDBTrigger")]
      public static void CosmosDBTrigger([CosmosDBTrigger(
            databaseName: "CosmicDB",
            containerName: "CosmicUsers",
            Connection = "CosmicDBIdentity",
            LeaseContainerName = "leasesCosmicUsers",
            CreateLeaseContainerIfNotExists =true)]IReadOnlyList<User> input,
          ILogger log)
      {
         if (input != null && input.Count > 0)
         {
            log.LogInformation("Documents modified " + input.Count);
            log.LogInformation("First document Id " + input[0].name);
         }
      }
   }
}
