using EarlyBoundTypes;
using Microsoft.Xrm.Sdk;
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


        public ActionResult Case(string caseID)
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
            cvm.Klient = new EntityReference("contact", incident.CustomerId.Id).Name;
            cvm.RodzajSzkody = incident.expl_Rodzajszkody.Name;
            cvm.Kontakt = "";
            cvm.NumerSprawy = incident.TicketNumber;
            cvm.Wlasciciel = incident.OwnerId.Name;
            cvm.TypSprawy = getOptionSetText("incident", "expl_typsprawy", (int)incident.expl_TypSprawy);
            cvm.ZrodloSprawy = getOptionSetText("incident", "expl_zrodlosprawy", (int)incident.expl_zrodlosprawy);

            ViewBag.Aps = aps;

            return View(cvm);
        }


    }
}