using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicChat.Shared.Extensions
{
   public static class DateTimeExtensions
   {
      public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
      {         
         DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
         dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
         return dateTime;
      }
      public static long GetPastHourTimestamp()
      {
         var currentUnixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
         var currentDate = UnixTimeStampToDateTime(currentUnixTimestamp);
         var currentMinutes = currentDate.Minute;
         return (currentUnixTimestamp - (currentMinutes * 60));
      }
   }
}
