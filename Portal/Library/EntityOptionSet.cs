using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Xml;
using System.Xml.Linq;
using PortalCRM.Library;

namespace Portal.Library
{
    public class EntityOptionSet : ConnectionContext
    {
        private readonly IDictionary<int, string> _optionSetLabelCache = new Dictionary<int, string>();

        public string GetEntityStatusLabel(string entityLogicalName, int statusCode)
        {
            // Create the attribute request (define which entities attribute
            //  information we want to get) entity.LogicalName is our current
            //  name of our entity (ex: contact, account, etc.)
            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = "statuscode",
                RetrieveAsIfPublished = true
            };

            // Execute the request to get the attribute information
            RetrieveAttributeResponse attributeResponse =
                (RetrieveAttributeResponse)XrmContext.Execute(attributeRequest);
            AttributeMetadata attrMetadata =
                    (AttributeMetadata)attributeResponse.AttributeMetadata;

            // Cast the AttributeMetadata to StatusAttribute data
            StatusAttributeMetadata statusAttrMetadata =
                (StatusAttributeMetadata)attrMetadata;

            // Get the status code label by comparing all of the status code values
            //  possible for the status code drop down list. Once we get a match on
            //  the value we take the label for that value 

            // For every status code value within all of our status codes values
            //  (all of the values in the drop down list)
            foreach (StatusOptionMetadata statusMeta in
                statusAttrMetadata.OptionSet.Options)
            {
                // Check to see if our current value matches
                if (statusMeta.Value == statusCode)
                {
                    // If our numeric value matches, set the string to our status code
                    //  label
                    return statusMeta.Label.UserLocalizedLabel.Label;
                }
            }

            // if we got this far, something didn't add up. 
            return string.Empty;
        }

        public string GetOptionSetLabelByValue(string entityLogicalName, string optionSetLogicalName, int value)
        {
            if (string.IsNullOrEmpty(entityLogicalName) || string.IsNullOrEmpty(optionSetLogicalName))
            {
                return string.Empty;
            }

            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = optionSetLogicalName
            };

            var retrieveAttributeResponse = (RetrieveAttributeResponse)XrmContext.Execute(retrieveAttributeRequest);

            var retrievedPicklistAttributeMetadata = (EnumAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            var option = retrievedPicklistAttributeMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == value);

            if (option == null)
            {
                return string.Empty;
            }

            var label = option.Label.UserLocalizedLabel.Label;

            if (option.Value.HasValue)
            {
                _optionSetLabelCache[option.Value.Value] = label;
            }

            return label;
        }

        public int GetOptionSetValueByLabel(string entityLogicalName, string optionSetLogicalName, string label)
        {
            if (string.IsNullOrEmpty(entityLogicalName) || string.IsNullOrEmpty(optionSetLogicalName))
            {
                return 0;
            }

            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = optionSetLogicalName
            };

            var retrieveAttributeResponse = (RetrieveAttributeResponse)XrmContext.Execute(retrieveAttributeRequest);
            var retrievedPicklistAttributeMetadata = (EnumAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            int value = 0;
            for (int i = 0; i < retrievedPicklistAttributeMetadata.OptionSet.Options.Count(); i++)
            {
                if (retrievedPicklistAttributeMetadata.OptionSet.Options[i].Label.LocalizedLabels[0].Label == label)
                    value = retrievedPicklistAttributeMetadata.OptionSet.Options[i].Value.Value;
            }

            return value;
        }

        public string GetGlobalOptionSetLabelByValue(string optionSetLogicalName, int value)
        {
            if (string.IsNullOrEmpty(optionSetLogicalName))
            {
                return string.Empty;
            }

            var retrieveAttributeRequest = new RetrieveOptionSetRequest
            {
                Name = optionSetLogicalName
            };

            var retrieveAttributeResponse = (RetrieveOptionSetResponse)XrmContext.Execute(retrieveAttributeRequest);

            var retrievedPicklistAttributeMetadata = (OptionSetMetadata)retrieveAttributeResponse.OptionSetMetadata;

            var option = retrievedPicklistAttributeMetadata.Options.FirstOrDefault(o => o.Value == value);

            if (option == null)
            {
                return string.Empty;
            }

            var label = option.Label.UserLocalizedLabel.Label;

            if (option.Value.HasValue)
            {
                _optionSetLabelCache[option.Value.Value] = label;
            }

            return label;
        }

        public int GetGlobalOptionSetValueByLabel(string optionSetLogicalName, string label)
        {
            if (string.IsNullOrEmpty(optionSetLogicalName))
            {
                return 0;
            }

            var retrieveAttributeRequest = new RetrieveOptionSetRequest
            {
                Name = optionSetLogicalName
            };

            var retrieveAttributeResponse = (RetrieveOptionSetResponse)XrmContext.Execute(retrieveAttributeRequest);
            var retrievedPicklistAttributeMetadata = (OptionSetMetadata)retrieveAttributeResponse.OptionSetMetadata;

            int value = 0;
            for (int i = 0; i < retrievedPicklistAttributeMetadata.Options.Count(); i++)
            {
                if (retrievedPicklistAttributeMetadata.Options[i].Label.LocalizedLabels[0].Label == label)
                    value = retrievedPicklistAttributeMetadata.Options[i].Value.Value;
            }

            return value;
        }

        public IDictionary<int, string> GetOptionSetValues(string entityLogicalName, string optionSetLogicalName)
        {
            IDictionary<int, string> optionSet = new Dictionary<int, string>();

            if (string.IsNullOrEmpty(entityLogicalName) || string.IsNullOrEmpty(optionSetLogicalName))
                return null;

            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = optionSetLogicalName
            };

            var retrieveAttributeResponse = (RetrieveAttributeResponse)XrmContext.Execute(retrieveAttributeRequest);
            var retrievedPicklistAttributeMetadata = (EnumAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            for (int i = 0; i < retrievedPicklistAttributeMetadata.OptionSet.Options.Count(); i++)
            {
                optionSet.Add(new KeyValuePair<int, string>(retrievedPicklistAttributeMetadata.OptionSet.Options[i].Value.Value,
                    retrievedPicklistAttributeMetadata.OptionSet.Options[i].Label.LocalizedLabels[0].Label));
            }

            return optionSet;
        }

        public IDictionary<int, string> GetGlobalOptionSetValues(string optionSetLogicalName)
        {
            IDictionary<int, string> optionSet = new Dictionary<int, string>();

            if (string.IsNullOrEmpty(optionSetLogicalName))
                return null;

            var retrieveAttributeRequest = new RetrieveOptionSetRequest
            {
                Name = optionSetLogicalName
            };

            var retrieveAttributeResponse = (RetrieveOptionSetResponse)XrmContext.Execute(retrieveAttributeRequest);
            var retrievedPicklistAttributeMetadata = (OptionSetMetadata)retrieveAttributeResponse.OptionSetMetadata;

            for (int i = 0; i < retrievedPicklistAttributeMetadata.Options.Count(); i++)
            {
                optionSet.Add(new KeyValuePair<int, string>(retrievedPicklistAttributeMetadata.Options[i].Value.Value,
                    retrievedPicklistAttributeMetadata.Options[i].Label.LocalizedLabels[0].Label));
            }

            return optionSet;
        }
    }
}