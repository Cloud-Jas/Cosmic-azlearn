using AzureFunctions.Extensions.Middleware;
using AzureFunctions.Extensions.Middleware.Abstractions;
using CosmicChat.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            CreateLeaseContainerIfNotExists =true)]IReadOnlyList<User> input)
      {

         if (input != null && input.Count > 0)
         {
            _logger.LogInformation("Documents modified " + input.Count);
            _logger.LogInformation("First document Id " + input[0].name);
         }

      }
   }
}
