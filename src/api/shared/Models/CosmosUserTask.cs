using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Models
{
   public class TaskDetail
   {
      public string userId { get; set; }
      public string userName { get; set; }
      public Country country { get; set; }
      public Street street { get; set; }
      public Position position { get; set; }
   }

   public class ValidateStatus
   {
      public bool isCompleted { get; set; }
      public List<string> chatIds { get; set; }
   }

   public class CosmosUserTask
   {
      public string id { get; set; }
      public string userId { get; set; }
      public string userName { get; set; }
      public TaskDetail taskDetail { get; set; }
      public bool isCompleted { get; set; }
      public long _ts { get; set; }
   }

}