using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Models
{
   public class CosmosUserValidator
   {
      public string id { get; set; }
      public string userId { get; set; }
      public string toUserId { get; set; }
      public long _ts { get; set; }
   }
}
