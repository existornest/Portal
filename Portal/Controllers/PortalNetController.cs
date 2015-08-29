﻿using Portal.Library.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EarlyBoundTypes;
using Portal.ViewModels;
using Portal.Models;
using Microsoft.Xrm.Sdk;

namespace Portal.Controllers
{
    public class PortalNetController : CommonController
    {
        // GET: PortalNet
        public ActionResult Index()
        {



            ViewBag.Html = Session["tree"];

            return View();
        }


        public ActionResult Cases(string guid, string advFunction)
        {

            //if(null == guid || null == advFunction)
            //{
            //    TempData["error"] = "Wystąpił błąd. Prosimy o kontact z administracją.";
            //    return RedirectToAction("Index", "Error");

            //}

            if (null == guid || null == advFunction)
            {
                return HttpNotFound();
            }

            IQueryable<Incident> tickets = null;

            switch (advFunction)
            {
                case "Dyrektor Regionu":
                    tickets = context
                        .IncidentSet.Where(a => a.expl_DyrektorRegionu.Id == new Guid(guid))
                        .Select(b => b);
                    break;
                case "Dyrektor Sprzedaży":
                    tickets = context
                        .IncidentSet.Where(a => a.expl_DyrSprzedazy.Id == new Guid(guid))
                        .Select(b => b);
                    break;
                case "Koordynator":
                    tickets = context
                        .IncidentSet.Where(a => a.expl_Koordynator.Id == new Guid(guid))
                        .Select(b => b);
                    break;
                case "Konsultant":
                    tickets = context
                        .IncidentSet.Where(a => a.expl_Konsultant.Id == new Guid(guid))
                        .Select(b => b);
                    break;
                default:
                    break;
            }


            IncidentModel im = new IncidentModel();

            foreach (Incident incident in tickets)
            {
                im.IncidentModelList.Add(new IncidentObject()
                {
                    ID = incident.Id,
                    Client = incident.CustomerId.Name,
                    LastActionDate = (incident.expl_Ostatniedzialanie.HasValue)? incident.expl_Ostatniedzialanie.Value.ToLocalTime().ToString("yyy-MM-dd") : null,
                    State = (incident.StateCode.HasValue)? ((int)incident.StateCode.Value == 1) ? "Aktywne" : "Nieaktywne" : null,
                    DateCreated = (incident.CreatedOn.HasValue)? incident.CreatedOn.Value.ToLocalTime().ToString("yyy-MM-dd") : null,
                    Ticket = incident.TicketNumber.ToString()
                });
            }

            ViewBag.Html = Session["tree"];
            ViewBag.Guid = guid;
            ViewBag.advFunction = advFunction;
            ViewBag.Tickets = tickets;

            return View(im);
        }


        public ActionResult SingleCase(string caseID, string client)
        {
            

            if (null == caseID || null == client)
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
            cvm.Klient = new EntityReference("contact", (Guid)incident.incident_customer_contacts.ContactId).Name;
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