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
      public long timestamp { get; set; }
   }
   public class UserDetail
   {
      public string id { get; set; }
      public User toUser { get; set; }
   }

   public class CosmosChat
   {
      public string id { get; set; }
      public Chat chat { get; set; }
      public List<UserDetail> userDetails { get; set; }
      public Message message { get; set; }
      public long _ts { get; set; }
   }


}
