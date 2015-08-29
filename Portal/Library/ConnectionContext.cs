using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Client;
using System;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.ServiceModel.Description;
using System.Collections.Specialized;
using Microsoft.Xrm;
using EarlyBoundTypes;

namespace PortalCRM.Library
{
    public class ConnectionContext
    {
        private readonly Lazy<XrmServiceContext> _xrmContext;

        public ConnectionContext()
        {
            _xrmContext = new Lazy<XrmServiceContext>(() => CreateXrmServiceContext());
        }

        /// <summary>
        /// A general use <see cref="OrganizationServiceContext"/> for managing entities on the page.
        /// </summary>
        public XrmServiceContext XrmContext
        {
            get { return _xrmContext.Value; }
        }

        protected XrmServiceContext CreateXrmServiceContext(MergeOption? mergeOption = null)
        {
            //string organizationUri = ConfigurationManager.AppSettings["CRM_OrganisationUri"];

            string organizationUri = "https://adversum.api.crm4.dynamics.com/XRMServices/2011/Organization.svc";

            IServiceManagement<IOrganizationService> OrganizationServiceManagement = ServiceConfigurationFactory.CreateManagement<IOrganizationService>(new Uri(organizationUri));
            AuthenticationProviderType OrgAuthType = OrganizationServiceManagement.AuthenticationType;
            AuthenticationCredentials authCredentials = GetCredentials(OrgAuthType);
            AuthenticationCredentials tokenCredentials = OrganizationServiceManagement.Authenticate(authCredentials);
            OrganizationServiceProxy organizationProxy = null;
            SecurityTokenResponse responseToken = tokenCredentials.SecurityTokenResponse;

            if (ConfigurationManager.AppSettings["CRM_AuthenticationType"] == "ActiveDirectory")
            {
                using (organizationProxy = new OrganizationServiceProxy(OrganizationServiceManagement, authCredentials.ClientCredentials))
                {
                    organizationProxy.EnableProxyTypes();
                }
            }
            else
            {
                using (organizationProxy = new OrganizationServiceProxy(OrganizationServiceManagement, responseToken))
                {
                    organizationProxy.EnableProxyTypes();
                }
            }

            IOrganizationService service = (IOrganizationService)organizationProxy;

            var context = new XrmServiceContext(service);
            if (context != null && mergeOption != null) context.MergeOption = mergeOption.Value;
            return context;
        }

        private AuthenticationCredentials GetCredentials(AuthenticationProviderType endpointType)
        {
            //Load the credentials from the Web.config first
            //string userName = ConfigurationManager.AppSettings["CRM_Username"];
            //string password = ConfigurationManager.AppSettings["CRM_Password"];
            //string domain = ConfigurationManager.AppSettings["CRM_Domain"];

            string userName = "SupportExisto@adversum.onmicrosoft.com";
            string password = "AdvCRM2013!";
            string domain = "";

            //Load the auth type
            string authenticationType = ConfigurationManager.AppSettings["CRM_AuthenticationType"];

            AuthenticationCredentials authCreds = new AuthenticationCredentials();

            switch (authenticationType)
            {
                case "ActiveDirectory":
                    authCreds.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(userName, password, domain);
                    break;
                case "LiveId":
                    authCreds.ClientCredentials.UserName.UserName = userName;
                    authCreds.ClientCredentials.UserName.Password = password;
                    authCreds.SupportingCredentials = new AuthenticationCredentials();
                    authCreds.SupportingCredentials.ClientCredentials =
                        Microsoft.Crm.Services.Utility.DeviceIdManager.LoadOrRegisterDevice();
                    break;
                case "Online": // For Federated and OnlineFederated environments.
                    authCreds.ClientCredentials.UserName.UserName = userName;
                    authCreds.ClientCredentials.UserName.Password = password;
                    break;
                case "SSO": //Single Sign On
                    // For OnlineFederated single-sign on, you could just use current UserPrincipalName instead of passing user name and password.
                    authCreds.UserPrincipalName = UserPrincipal.Current.UserPrincipalName; //Windows/Kerberos
                    break;
                default: // Online                    
                    authCreds.ClientCredentials.UserName.UserName = userName;
                    authCreds.ClientCredentials.UserName.Password = password;
                    break;
            }

            return authCreds;
        }
    }
}