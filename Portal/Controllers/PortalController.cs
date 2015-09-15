using EarlyBoundTypes;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Portal.Library.Controllers;
using Portal.ViewModels;
using PortalCRM.Library;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;


namespace Portal.Controllers
{
    public class PortalController : CommonController
    {

        // GET: Portal
        public ActionResult Index()
        {

            Contact user = (Contact)
                context.Retrieve(
                "contact", (Guid)Session["guid"], new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            IQueryable<Incident> incidents =
                context.IncidentSet
                .Where(a => a.CustomerId.Equals(user.ContactId))
                .Select(row => row);

            string userName = user.FullName;

            ViewBag.User = userName;

            
            List<IncidentsViewModel> IncidentsList = new List<IncidentsViewModel>();

            foreach (Incident item in incidents)
            {
                IncidentsViewModel ivm = new IncidentsViewModel();
                ivm.IncidentID = item.IncidentId.ToString();
                ivm.CaseNumber = item.TicketNumber;
                ivm.Client = item.CustomerId.Name;
                ivm.DateOfCreation = item.CreatedOn.HasValue ? item.CreatedOn.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "";
                ivm.State = item.StateCode.Value == 1 ? "Aktywna" : "Nieaktywna";

                IncidentsList.Add(ivm);

            }

            ViewBag.Incidents = incidents;

            #region codeBehind
            //var xrm = new XrmServiceContext("Xrm");
            //var cases = xrm.IncidentSet;
            //if (Session["field2"] == null)
            //{
            //    if (Session["czyklient"] != null)
            //    {
            //        cases = xrm.IncidentSet
            //           .Where(c => ((c.CustomerId.Id == Contact.ContactId) || (c.expl_Przedstawiciel.Id == Contact.ContactId)) && c.IncidentStageCode.Value != 969330000);
            //    }
            //    else if (Session["czykonto"] != null)
            //    {
            //        cases = xrm.IncidentSet
            //               .Where(c => (c.CustomerId.Id == kontoid) && c.IncidentStageCode.Value != 969330000);
            //    }
            //    else
            //    {
            //        cases = xrm.IncidentSet
            //             .Where(c => ((c.CustomerId.Id == Contact.ContactId) || (c.expl_Przedstawiciel.Id == Contact.ContactId)) && c.IncidentStageCode.Value != 969330000);
            //    }
            //}
            //else

            //    cases = xrm.IncidentSet
            //        .Where(c => (c.expl_Polisa.Id == (Guid)Session["field2"]));

            #endregion codeBehind


            return View(IncidentsList);
        }


        public ActionResult Case(string caseID, string client)
        {

            if (null == caseID)
            {
                return HttpNotFound();
            }

            Incident incident =
            context.IncidentSet
                .Where(a => a.IncidentId.Value == new Guid(caseID)).Select(row => row).FirstOrDefault();



            IEnumerable<ActivityPointer> aps = incident.Incident_ActivityPointers
               .Where(c => (c.PriorityCode == 2 && c.StateCode.Value == 1 && c.ActivityTypeCode != "email"))
               .OrderByDescending(d => (d.ActualEnd));

            CaseViewModel cvm = new CaseViewModel();
            cvm.DataWplywu = incident.CreatedOn.GetValueOrDefault().ToLocalTime().ToString("yyyy-MM-dd");
            cvm.DatawyslaniaUmowy = incident.expl_Datawysaniaumowy.GetValueOrDefault().ToLocalTime().ToString("yyyy-MM-dd");
            cvm.EtapSprawy = getOptionSetText("incident", "incidentstagecode", (int)incident.IncidentStageCode);
            cvm.Guid = (Guid)incident.IncidentId;
            cvm.Klient = incident.CustomerId.Name;
            cvm.RodzajSzkody = incident.expl_Rodzajszkody.Name;
            cvm.Kontakt = new EntityReference("contact", incident.CustomerId.Id).Name;
            cvm.NumerSprawy = incident.TicketNumber;
            cvm.Wlasciciel = incident.OwnerId.Name;
            cvm.TypSprawy = getOptionSetText("incident", "expl_typsprawy", (int)incident.expl_TypSprawy);
            cvm.ZrodloSprawy = getOptionSetText("incident", "expl_zrodlosprawy", (int)incident.expl_zrodlosprawy);

            ViewBag.Aps = aps;
            ViewBag.CaseID = caseID;
            return View(cvm);
        }

        public void Instruction()
        {

            string filename = "instrukcja.doc";
            if (filename != "")
            {
                //string path ="C:\\Users\\pjurkun\\Desktop\\customer\\CustomerPortal\\CustomerPortal\\Web\\Pages\\eService\\instrukcja.doc";
                string path = Server.MapPath("~/App_Data/instrukcja.doc");
                System.IO.FileInfo file = new System.IO.FileInfo(path);
                if (file.Exists)
                {
                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);
                    Response.AddHeader("Content-Length", file.Length.ToString());
                    Response.ContentType = "application/octet-stream";
                    Response.WriteFile(file.FullName);
                    Response.End();
                }
                else
                {
                    Response.Write(path);
                }
            }

            //return null;
        }

        public ActionResult Policies()
        {
            Guid currentUserGuid = new Guid();
            currentUserGuid = (Guid)Session["guid"];

            string policies = "";

            PoliciesViewModel pvm = new PoliciesViewModel();
            //List<PoliciesViewModel> lpvm = new List<PoliciesViewModel>();

            // logowanie jako kontakt
            if (1 == (int)Session["isContact"])
            {
                //80fa1356-9fb2-e411-9184-d89d67634d30
                //a55c46b5-4233-e411-be17-2c59e5414d14 - Wrzosek, Danuta
                policies = string.Format(@"
                            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >
                                <entity name = 'expl_polisa' >
                                    <attribute name = 'expl_name' />
                                    <attribute name = 'expl_rodzajubezpieczenia' />
                                    <attribute name = 'expl_konto' />
                                    <attribute name = 'expl_kontakt' />
                                    <attribute name = 'expl_klient' />
                                    <attribute name = 'expl_firmaubezpieczeniowa' />
                                    <attribute name = 'expl_okresubezpieczeniastrat' />
                                    <attribute name = 'expl_okresubezpieczeniakoniec' />
                                    <attribute name = 'expl_datapolisy' />
                                    <attribute name = 'expl_polisaid' />
                                    <order attribute = 'expl_name' descending = 'false' />
                                    <filter type = 'and' >
                                        <condition attribute = 'expl_kontakt' operator= 'eq' uitype = 'contact' value = '{0}' />
                                    </filter >
                                </entity >
                            </fetch >", currentUserGuid);

                

                EntityCollection policiesCollection = context.RetrieveMultiple(new FetchExpression(policies));

                PoliciesObject po = null;

                if (policiesCollection.Entities.Count > 0)
                {

                    foreach (expl_polisa item in policiesCollection.Entities)
                    {
                        po = new PoliciesObject()
                        {
                            Account = (item.expl_konto != null) ? item.expl_konto.Name : "",
                            Contact = (item.expl_kontakt != null) ? item.expl_kontakt.Name : "",
                            Company = (item.expl_firmaubezpieczeniowa != null) ? item.expl_firmaubezpieczeniowa.Name : "",
                            InsuranceNumber = (item.expl_name != null) ? item.expl_name : "",
                            DateStart = item.expl_okresubezpieczeniastrat.HasValue ? item.expl_okresubezpieczeniastrat.Value.ToLocalTime().ToString("yyyy-MM-dd") : "",
                            DateEnd = item.expl_okresubezpieczeniastrat.HasValue ? item.expl_okresubezpieczeniakoniec.Value.ToLocalTime().ToString("yyyy-MM-dd") : "",
                            InsuranceType = (item.expl_rodzajubezpieczenia.HasValue) ? getOptionSetText("expl_polisa", "expl_rodzajubezpieczenia", item.expl_rodzajubezpieczenia.Value) : "",

                        };
                        pvm.PoliciesList.Add(po);
                    }

                    

                }



            }
            // logowanie jako konto
            else if (0 == (int)Session["isContact"])
            {
                Account ac = (Account)Session["Account"];

                policies = "";

                if(null != ac)
                {
                    policies = string.Format(@" 
                        <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                            <entity name='account'> 
                                <attribute name='accountid'   /> 
                           <filter type='and'>
                             <condition attribute='primarycontactid' value='{0}' uitype='contact' operator='eq'/>
                           </filter>
                            </entity>
                        </fetch>", ac.AccountId.ToString());
                }
                
                

                EntityCollection accounts = null;

                try
                {
                    accounts = context.RetrieveMultiple(new FetchExpression(policies));
                }
                catch
                {

                    //return RedirectToAction("");
                }

                if (accounts != null && accounts.Entities.Count > 0)
                {

                    foreach (Account a in accounts.Entities)
                    {
                        string accountPolicies = string.Format(@" 
                        <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                            <entity name='expl_polisa'> 
                                <attribute name='expl_name'   /> 
                                <attribute name='expl_konto'   /> 
                                <attribute name='expl_kontakt'   /> 
                                <attribute name='expl_dataumowy'   /> 
                                <attribute name='expl_rodzajubezpieczenia'   /> 
                                <attribute name='expl_firmaubezpieczeniowa'   />     
                                <attribute name='expl_okresubezpieczeniastrat'   />
                                <attribute name='expl_okresubezpieczeniakoniec'   />
                           <filter type='and'>
                             <condition attribute='expl_konto' value='{0}' uitype='account' operator='eq'/>
                           </filter>
                            </entity>
                        </fetch>", a.Id);


                        EntityCollection accountPoliciesCollection = context.RetrieveMultiple(new FetchExpression(accountPolicies));

                        PoliciesObject po = null;

                        foreach (expl_polisa item in accountPoliciesCollection.Entities)
                        {
                            po = new PoliciesObject()
                            {
                                Account = (item.expl_konto != null) ? item.expl_konto.Name : "",
                                Contact = (item.expl_kontakt != null) ? item.expl_kontakt.Name : "",
                                Company = (item.expl_firmaubezpieczeniowa != null) ? item.expl_firmaubezpieczeniowa.Name : "",
                                InsuranceNumber = (item.expl_name != null) ? item.expl_name : "",
                                DateStart = item.expl_okresubezpieczeniastrat.HasValue ? item.expl_okresubezpieczeniastrat.Value.ToLocalTime().ToString("yyyy-MM-dd") : "",
                                DateEnd = item.expl_okresubezpieczeniastrat.HasValue ? item.expl_okresubezpieczeniakoniec.Value.ToLocalTime().ToString("yyyy-MM-dd") : "",
                                InsuranceType = (item.expl_rodzajubezpieczenia.HasValue) ? getOptionSetText("expl_polisa", "expl_rodzajubezpieczenia", item.expl_rodzajubezpieczenia.Value) : "",

                            };
                        }

                        pvm.PoliciesList.Add(po);

                    }


                }


            }
            



            return View(pvm);
        }


        public ActionResult Documents(string caseID)
        {

            if (null == caseID)
            {
                return HttpNotFound();
            }

            string tekst = null;
            //string fileName = null;

            DocumentViewModel docsList = new DocumentViewModel();

            var url = context.SharePointDocumentLocationSet.Where(c => c.RegardingObjectId.Id == new Guid(caseID));
            if (url != null)
            {


                foreach (SharePointDocumentLocation b in url)
                {
                    tekst = b.RelativeUrl.ToString();

                    RetrieveAbsoluteAndSiteCollectionUrlRequest retrieveRequest = new RetrieveAbsoluteAndSiteCollectionUrlRequest
                    {
                        Target = new EntityReference(SharePointDocumentLocation.EntityLogicalName, b.Id)
                    };

                    RetrieveAbsoluteAndSiteCollectionUrlResponse retriveResponse =
                        (RetrieveAbsoluteAndSiteCollectionUrlResponse)context.Execute(retrieveRequest);
                }

                var targetSite = new Uri("https://adversum.sharepoint.com");
                var login = ConfigurationManager.AppSettings["userName"];
                var password = ConfigurationManager.AppSettings["password"];

                var securePassword = new SecureString();

                foreach (char c in password)
                {
                    securePassword.AppendChar(c);
                }

                var onlineCredentials = new SharePointOnlineCredentials(login, securePassword);

                using (ClientContext clientContext = new ClientContext(targetSite))
                {
                    clientContext.Credentials = onlineCredentials;

                    if (tekst != null)
                    {

                        string path = tekst + "/incident/" + tekst.ToString() + "/Klient";
                        Folder folder = clientContext.Web.GetFolderByServerRelativeUrl("/incident/" + tekst.ToString() + "/Klient");

                        FileCollection files = folder.Files;

                        clientContext.Load(folder);
                        clientContext.Load(files);



                        try
                        {

                            clientContext.ExecuteQuery();

                            int temp = 1;



                            foreach (var file in files)
                            {

                                docsList.DocsList.Add(new DocumentsViewObject()
                                {
                                    CaseID = caseID,
                                    FileName = file.Name,
                                    Url = file.ServerRelativeUrl.ToString(),
                                    DateOf = file.TimeLastModified.ToShortDateString()
                                });

                                #region example
                                //var cell = new TableCell();
                                //var row = new TableRow();
                                //if (temp == 1)
                                //{
                                //    cell.Font.Bold = true;
                                //    cell.Text = "Plik";
                                //    row.Cells.Add(cell);
                                //    cell = new TableCell();
                                //    cell.Font.Bold = true;
                                //    cell.Text = "Data dodania";
                                //    row.Cells.Add(cell);
                                //    casetable.Rows.Add(row);
                                //}
                                //row = new TableRow();
                                //cell = new TableCell();

                                //LinkButton myLabel = new LinkButton();
                                //myLabel.Text = file.Name.ToString();
                                //myLabel.ID = "Label" + temp.ToString();
                                //myLabel.CommandArgument = file.ServerRelativeUrl.ToString();
                                //myLabel.Click += new EventHandler(Clicked);
                                //fileName = file.Name.ToString();
                                //panelukas.Controls.Add(myLabel);
                                //panelukas.Controls.Add(new LiteralControl("<br />"));

                                //cell.Controls.Add(myLabel);
                                //row.Cells.Add(cell);

                                //cell = new TableCell();
                                //cell.Text = file.TimeLastModified.ToShortDateString();
                                //row.Cells.Add(cell);
                                //casetable.Rows.Add(row);
                                #endregion example


                                temp++;
                            }
                            if (temp == 1)
                            {
                               
                                ViewBag.InfoDocs = "Brak dokumentów przypisanych do sprawy";
                            }
                        }
                        catch (Exception)
                        {
                              
                            ViewBag.InfoDocs = "Brak dokumentów przypisanych do sprawy";
                        }
                    }
                    else
                    {
                        
                        ViewBag.InfoDocs = "Brak dokumentów przypisanych do sprawy";
                    }
                }
            }
            else
            {
                ViewBag.InfoDocs = "Brak dokumentów przypisanych do sprawy";
            }

            ViewBag.CaseID = caseID;

            return View(docsList);

        }

        public void SingleDocument(string name, string command)
        {

            string fileName = name;
            string commandStr = command;


            var targetSite = new Uri("https://adversum.sharepoint.com");
            var login = ConfigurationManager.AppSettings["userName"];
            var password = ConfigurationManager.AppSettings["password"];

            var securePassword = new SecureString();

            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }

            var onlineCredentials = new SharePointOnlineCredentials(login, securePassword);

            using (ClientContext clientContext = new ClientContext(targetSite))
            {
                clientContext.Credentials = onlineCredentials;
                Folder folder = clientContext.Web.GetFolderByServerRelativeUrl("/incident/" + fileName);

                FileCollection files = folder.Files;

                clientContext.Load(folder);
                clientContext.Load(files);
                clientContext.ExecuteQuery();

                FileInformation fi = Microsoft.SharePoint.Client.File.OpenBinaryDirect(clientContext, commandStr.ToString());//file.ServerRelativeUrl.ToString());

                MemoryStream memoryStream = new MemoryStream();

                fi.Stream.CopyTo(memoryStream);
                fi.Stream.Close();

                byte[] btFile = memoryStream.ToArray();
                memoryStream.Close();

                Response.AddHeader("Content-disposition", "attachment; filename=" + fileName);
                Response.ContentType = "application/octet-stream";
                Response.BinaryWrite(btFile);
                Response.End();

            }


        }

    }
}
