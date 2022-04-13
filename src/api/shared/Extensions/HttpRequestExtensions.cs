using CosmicChat.Shared.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Extensions
{
   public static class HttpRequestExtensions
   {
      public static CosmosUser ParseUser(this HttpRequest req)
      {
         var principal = new ClientPrincipal();

         if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
         {
            var data = header[0];
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.ASCII.GetString(decoded);
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
         }

         var user = new CosmosUser
         {
            id = principal?.UserId,
            name = principal?.UserDetails
         };

         return user;
      }
   }
}
