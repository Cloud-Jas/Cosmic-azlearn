using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.API.Models
{
   // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
   public class Country
   {
      public string code { get; set; }
      public string subDivision { get; set; }
      public string secondarySubDivision { get; set; }
      public string isoCode3 { get; set; }
   }

   public class Street
   {
      public string name { get; set; }
      public string nameAndNumber { get; set; }
   }

   public class Position
   {
      public string latitude { get; set; }
      public string longitude { get; set; }
   }

   public class Address
   {
      public string name { get; set; }
      public Country country { get; set; }
      public Street street { get; set; }
      public Position position { get; set; }
   }

   public class User
   {
      public string identityProvider { get; set; }
      public string id { get; set; }
      public string name { get; set; }
      public List<string> roles { get; set; }
      public Address address { get; set; }
   }


}
