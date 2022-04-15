using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Models
{   
   public class CosmosLeaderboard
   {
      public string id { get; set; }
      public User user { get; set; }
      public LeaderboardTask task { get; set; }
      public int score { get; set; }
      public int _ts { get; set; }
   }

   public class LeaderboardTask
   {
      public string id { get; set; }      
   }

}
