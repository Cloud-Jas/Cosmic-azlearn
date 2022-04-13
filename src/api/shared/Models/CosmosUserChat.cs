using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Models
{
   public class CosmosUserChat
   {
      public string id { get; set; }
      public string userId { get; set; }
      public string toUserId { get; set; }
      public string senderId { get; set; }      
      public string chatId { get; set; }
      public string chatName { get; set; }
      public string lastMessage { get; set; }
      public long lastMessageTimestamp { get; set; }
   }
}
