using EarlyBoundTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Library.Helpers
{
    public static class CaseHelper
    {
        public static string TypeString(string type)
        {
            string modified = "";
            switch (type)
            {
                case "task":
                    modified = "Zadanie";
                    break;

                case "phonecall":
                    modified = "Rozmowa tel";
                    break;

                case "letter":
                    modified = "List";
                    break;

                default:
                    modified = "task";
                    break;
            }

            return modified;
        }


        public static string DateString(ActivityPointer a)
        {

            string result = "";

            if (a.ScheduledEnd != null)
            {
                if (a.CreatedOn <= a.ScheduledEnd)
                {
                    result = a.ScheduledEnd.GetValueOrDefault().ToLocalTime().ToString("yyyy-MM-dd");
                }
                else
                {
                    result = a.ActualEnd.GetValueOrDefault().ToLocalTime().ToString("yyyy-MM-dd");
                }
                    
            }
            else
            {
                result = a.ActualEnd.GetValueOrDefault().ToLocalTime().ToString("yyyy-MM-dd");
            }
                

            return result;
        }


    }
}