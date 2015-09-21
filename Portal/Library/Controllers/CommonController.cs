using EarlyBoundTypes;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Portal.Library.Controllers.Abstract;
using Portal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Portal.Library.Controllers
{
    public class CommonController : ABaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (null == Session["loggedUser"])
            {
                TempData["loginError"] = "Zaloguj się lub zarejestruj.";
                filterContext.Result = new RedirectResult("/Login");
            }
            
        }

       

        public int getOptionSetValue(string entityName, string attributeName, string optionsetText)
        {
            int optionSetValue = 0;
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
            retrieveAttributeRequest.EntityLogicalName = entityName;
            retrieveAttributeRequest.LogicalName = attributeName;
            retrieveAttributeRequest.RetrieveAsIfPublished = true;

            RetrieveAttributeResponse retrieveAttributeResponse =
              (RetrieveAttributeResponse)context.Execute(retrieveAttributeRequest);
            PicklistAttributeMetadata picklistAttributeMetadata =
              (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            OptionSetMetadata optionsetMetadata = picklistAttributeMetadata.OptionSet;

            foreach (OptionMetadata optionMetadata in optionsetMetadata.Options)
            {
                if (optionMetadata.Label.UserLocalizedLabel.Label.ToString() == optionsetText)
                {
                    optionSetValue = optionMetadata.Value.Value;
                    return optionSetValue;
                }

            }
            return optionSetValue;
        }

        public string getOptionSetText(string entityName, string attributeName, int optionsetValue)
        {
            string optionsetText = string.Empty;
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
            retrieveAttributeRequest.EntityLogicalName = entityName;
            retrieveAttributeRequest.LogicalName = attributeName;
            retrieveAttributeRequest.RetrieveAsIfPublished = true;

            RetrieveAttributeResponse retrieveAttributeResponse =
              (RetrieveAttributeResponse)context.Execute(retrieveAttributeRequest);
            PicklistAttributeMetadata picklistAttributeMetadata =
              (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            OptionSetMetadata optionsetMetadata = picklistAttributeMetadata.OptionSet;

            foreach (OptionMetadata optionMetadata in optionsetMetadata.Options)
            {
                if (optionMetadata.Value == optionsetValue)
                {
                    optionsetText = optionMetadata.Label.UserLocalizedLabel.Label;
                    return optionsetText;
                }

            }
            return optionsetText;
        }

        public OptionMetadataCollection getOptionSetValues(string entityName, string attributeName)
        {

            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
            retrieveAttributeRequest.EntityLogicalName = entityName;
            retrieveAttributeRequest.LogicalName = attributeName;
            retrieveAttributeRequest.RetrieveAsIfPublished = true;

            RetrieveAttributeResponse retrieveAttributeResponse =
              (RetrieveAttributeResponse)context.Execute(retrieveAttributeRequest);
            PicklistAttributeMetadata picklistAttributeMetadata =
              (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            OptionSetMetadata optionsetMetadata = picklistAttributeMetadata.OptionSet;


            return optionsetMetadata.Options;
        }

        public DateTime? GetPolishDateTimeNow()
        {

            DateTime polishDateTime = new DateTime();

            string timeZoneName = "Central European Standard Time";

            try
            {

                DateTime localTimeZone =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                TimeZoneInfo getZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
                polishDateTime = TimeZoneInfo.ConvertTime(localTimeZone, TimeZoneInfo.Local, getZone);
                
            }
            catch (Exception)
            {
                return null;
            }


            return polishDateTime.ToLocalTime();
        }

        public DateTime? ToPolishDateTime(DateTime? tdt)
        {

            DateTime polishDateTime = new DateTime();

            string timeZoneName = "Central European Standard Time";

            try
            {

                TimeZoneInfo getZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
                polishDateTime = TimeZoneInfo.ConvertTimeFromUtc(tdt.Value, getZone);


            }
            catch (Exception)
            {
                return tdt;
            }


            return polishDateTime;

        }



        public string GetBaseUrl()
        {
            var request = HttpContext.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;


            if (!string.IsNullOrWhiteSpace(appUrl)) appUrl += "/";


            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);
            return baseUrl;
        }


    }
}