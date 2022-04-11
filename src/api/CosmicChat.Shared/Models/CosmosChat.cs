using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Models
{
   public class Chat
   {
      public string id { get; set; }
   }

   public class Message
   {
      public string senderId { get; set; }
      public string content { get; set; }
      public int timestamp { get; set; }
   }

   public class CosmosChat
   {
      public string id { get; set; }
      public Chat chat { get; set; }
      public List<string> userIds { get; set; }
      public Message message { get; set; }
      public int _ts { get; set; }
   }


}
