using EarlyBoundTypes;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Portal.Library.Controllers;
using Portal.ViewModels;
using PortalCRM.Library;
using System;
using System.Collections.Generic;
using System.Linq;
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


            return View();
        }


        public ActionResult Case(string caseID, string client)
        {

            if (null == caseID)
            {
                return HttpNotFound();
            }

            Incident incident =
            context.IncidentSet
                .Where(a => a.IncidentId.Value.Equals(caseID)).Select(row => row).FirstOrDefault();

            

            IEnumerable<ActivityPointer> aps = incident.Incident_ActivityPointers
               .Where(c => (c.PriorityCode == 2 && c.StateCode.Value == 1 && c.ActivityTypeCode != "email"))
               .OrderByDescending(d => (d.ActualEnd));

            CaseViewModel cvm = new CaseViewModel();
            cvm.DataWplywu = incident.CreatedOn.GetValueOrDefault().ToLocalTime().ToString("yyyy-MM-dd");
            cvm.DatawyslaniaUmowy = incident.expl_Datawysaniaumowy.GetValueOrDefault().ToLocalTime().ToString("yyyy-MM-dd");
            cvm.EtapSprawy = getOptionSetText("incident", "incidentstagecode", (int)incident.IncidentStageCode);
            cvm.Guid = (Guid)incident.IncidentId;
            //cvm.Klient = (string)Session["loggedUser"];
            cvm.Klient = client;
            cvm.RodzajSzkody = incident.expl_Rodzajszkody.Name;
            cvm.Kontakt = new EntityReference("contact", incident.CustomerId.Id).Name;
            cvm.NumerSprawy = incident.TicketNumber;
            cvm.Wlasciciel = incident.OwnerId.Name;
            cvm.TypSprawy = getOptionSetText("incident", "expl_typsprawy", (int)incident.expl_TypSprawy);
            cvm.ZrodloSprawy = getOptionSetText("incident", "expl_zrodlosprawy", (int)incident.expl_zrodlosprawy);

            ViewBag.Aps = aps;

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

            string policies = string.Format(@" 
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
                             <condition attribute='expl_kontakt' value='{0}' uitype='contact' operator='eq'/>
                           </filter>
                            </entity>
                        </fetch>", Session["guid"]);

            EntityCollection policiesCollection = context.RetrieveMultiple(new FetchExpression(policies));

            foreach (expl_polisa ent in policiesCollection.Entities)
            {

            }


            return View();
        }


    }
}