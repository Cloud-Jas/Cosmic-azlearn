using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Models
{
   public class WebPubSubOptions
   {
      public string Endpoint { get; set; }
      public string Hub { get; set; }
      public string AccessKey { get; set; }
      public string ExpireAt { get; set; }
      public List<string> Roles { get; set; } = new List<string>() { "webpubsub.joinLeaveGroup", "webpubsub.sendToGroup" };
   }
}
