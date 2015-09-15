using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EarlyBoundTypes;

namespace Portal.Library.Controllers
{
    public static class ExtensionsTools
    {
        public static DateTime? ToPolishDateTime(this expl_prowizja provision)
        {
            
            DateTime polishDateTime = new DateTime();

            string timeZoneName = "Central European Standard Time";

            TimeZoneInfo getZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
            polishDateTime = TimeZoneInfo.ConvertTimeFromUtc(provision.expl_Data.Value, getZone);

            try
            {

                //DateTime localTimeZone =
                //new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                //DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                

            }
            catch (Exception)
            {
                return provision.expl_Data;
            }


            return polishDateTime;

        }
    }
}