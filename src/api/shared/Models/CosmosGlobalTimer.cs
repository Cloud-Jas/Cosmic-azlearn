using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Models
{
   public class Timestamp
   {
      public int startTimestampInSeconds { get; set; }
      public int _7DaysTimestampInSeconds { get; set; }
      public int _30DaysTimestampInSeconds { get; set; }
   }

   public class CosmosGlobalTimer
   {
      public string id { get; set; }
      public Timestamp timestamp { get; set; }
      public int _ts { get; set; }
   }
}
