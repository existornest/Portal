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
using System.Diagnostics;
using System.Security;
using Microsoft.SharePoint.Client;
using System.IO;
using Microsoft.Crm.Sdk.Messages;

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

        //Sprawy
        public ActionResult Cases(string guid, string advFunction, string ad = null, string sd = null, string cd = null)
        {


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
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);

                    break;
                case "Dyrektor Sprzedaży":

                    if (null != ad)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
                        .Where(a => a.expl_DyrSprzedazy.Id == new Guid(guid))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => a.expl_DyrSprzedazy.Id == new Guid(guid))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }

                    break;
                case "Koordynator":

                    if (null != ad && null != sd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_DyrektorRegionu.Id == new Guid(ad)))
                        .Where(a => (a.expl_DyrSprzedazy.Id == new Guid(sd)))
                        .Where(a => (a.expl_Koordynator.Id == new Guid(guid)))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else if (null == ad && null != sd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_DyrSprzedazy.Id == new Guid(sd)))
                        .Where(a => (a.expl_Koordynator.Id == new Guid(guid)))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_Koordynator.Id == new Guid(guid)))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }

                    break;
                case "Konsultant":

                    if (null != ad && null != sd && null != cd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_DyrektorRegionu.Id == new Guid(ad)))
                        .Where(a => (a.expl_DyrSprzedazy.Id == new Guid(sd)))
                        .Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
                        .Where(a => (a.expl_Konsultant.Id == new Guid(guid)))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else if (null == ad && null != sd && null != cd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_DyrSprzedazy.Id == new Guid(sd)))
                        .Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
                        .Where(a => (a.expl_Konsultant.Id == new Guid(guid)))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else if (null == ad && null == sd && null != cd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
                        .Where(a => (a.expl_Konsultant.Id == new Guid(guid)))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }

                    else
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_Konsultant.Id == new Guid(guid)))
                        .OrderByDescending(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }

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
                    LastActionDate = (incident.expl_Ostatniedzialanie.HasValue) ? incident.expl_Ostatniedzialanie.Value.ToLocalTime().ToString("yyy-MM-dd") : null,
                    State = (incident.StateCode.HasValue) ? ((int)incident.StateCode.Value == 1) ? "Aktywne" : "Nieaktywne" : null,
                    DateCreated = (incident.CreatedOn.HasValue) ? incident.CreatedOn.Value.ToLocalTime().ToString("yyy-MM-dd") : null,
                    Ticket = incident.TicketNumber.ToString()
                });
            }

            ViewBag.Html = Session["tree"];
            ViewBag.Guid = guid;
            ViewBag.advFunction = advFunction;

            return View(im);
        }


        public ActionResult SingleCase(string caseID, string client)
        {

            if (null == caseID || null == client)
            {
                return HttpNotFound();
            }

            Session["caseID"] = caseID;
            Session["client"] = client;
  
            Incident incident =
            context.IncidentSet
                .Where(a => a.IncidentId.Value.Equals(caseID)).Select(row => row).FirstOrDefault();


            IEnumerable<ActivityPointer> aps = incident.Incident_ActivityPointers
               .Where(c => (c.PriorityCode == 2 && c.StateCode.Value == 1 && c.ActivityTypeCode != "email"))
               .OrderByDescending(d => (d.ActualEnd));

            CaseViewModel cvm = new CaseViewModel();
            cvm.DataWplywu = incident.CreatedOn.HasValue ? incident.CreatedOn.Value.ToLocalTime().ToString("yyyy-MM-dd") : "";
            cvm.DatawyslaniaUmowy = incident.expl_Datawysaniaumowy.HasValue ? incident.expl_Datawysaniaumowy.Value.ToLocalTime().ToString("yyyy-MM-dd") : "";
            cvm.EtapSprawy = getOptionSetText("incident", "incidentstagecode", (int)incident.IncidentStageCode);
            cvm.Guid = (Guid)incident.IncidentId;
            cvm.Klient = incident.CustomerId.Name;
            cvm.RodzajSzkody = incident.expl_Rodzajszkody.Name;
            cvm.Kontakt = incident.PrimaryContactId != null ? incident.PrimaryContactId.Name : "";
            cvm.NumerSprawy = incident.TicketNumber;
            cvm.Wlasciciel = incident.OwnerId.Name;
            cvm.TypSprawy = getOptionSetText("incident", "expl_typsprawy", (int)incident.expl_TypSprawy);
            cvm.ZrodloSprawy = getOptionSetText("incident", "expl_zrodlosprawy", (int)incident.expl_zrodlosprawy);

            ViewBag.Aps = aps;
            ViewBag.Client = client;

            return View(cvm);
        }

        // Rozliczenia sieci - sprawa
        public ActionResult NetSettlement(string caseID, string client)
        {

            if (null == caseID || null == client)
            {
                return HttpNotFound();
            }

            List<SelectListItem> monthsList = new List<SelectListItem>();

            int currentYear = DateTime.Now.Year;

            List<SelectListItem> yearsList = new List<SelectListItem>();

            for (int i = (currentYear - 5), y = 1; i <= (currentYear); i++, y++)
            {
                if (i == currentYear)
                {
                    yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString(), Selected = true });
                }
                else
                {
                    yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString() });
                }
            }

            string[] months = {
                "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
                "Lipiec" ,"Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
            };

            for (int i = 0; i < 12; i++)
            {
                if (i == 0)
                {
                    monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString(), Selected = true });
                }
                else
                {
                    monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString() });
                }
            }

            IQueryable<expl_prowizja> provisionSet = null;

            provisionSet =
                 context.expl_prowizjaSet.Where(c => c.expl_Sprawa.Id == new Guid(caseID))
                 //.Where(c => c.expl_Kontakt.Id == (Guid)Session["guid"])
                 .Select(c => c);


            NetSettlementViewModel nsvm = new NetSettlementViewModel();

            foreach (expl_prowizja elem in provisionSet)
            {
                nsvm.NetSettlementList.Add(new NetSettlementObject()
                {
                    Account = (Decimal)elem.expl_Kwota,
                    Name = new EntityReference(elem.expl_Sprawa.LogicalName, elem.expl_Sprawa.Id).Name,
                    DateOf = elem.expl_Data.Value.ToLocalTime().ToString("yyyy-MM-dd"),
                    Source = getOptionSetText("expl_prowizja", "expl_tytulem", elem.expl_Tytulem.Value)
                });

            }

            ViewBag.Client = client;
            ViewBag.Months = monthsList;
            ViewBag.Years = yearsList;
            ViewBag.CaseID = caseID;
            ViewBag.Client = client;

            return View(nsvm);
        }

        // Rozliczenia sieci - sprawa
        [HttpPost]
        public ActionResult NetSettlement(NetSettlementViewModel vm, string caseID, string year, string month, string client = null)
        {

            if (null == caseID || null == year || null == month)
            {
                return HttpNotFound();
            }

            int currentYear = DateTime.Now.Year;
            int[] years = new int[6];

            for (int i = (currentYear - 5), y = 0; i <= (currentYear); i++, y++)
            {
                years[y] = i;
            }

            List<SelectListItem> monthsList = new List<SelectListItem>();
            List<SelectListItem> yearsList = new List<SelectListItem>();

            for (int i = (currentYear - 5), y = 1; i <= (currentYear); i++, y++)
            {
                if (i == currentYear)
                {
                    yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString(), Selected = true });
                }
                else
                {
                    yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString() });
                }
            }

            string[] months = {
                "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
                "Lipiec" ,"Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
            };

            for (int i = 0; i < 12; i++)
            {
                if (i == 0)
                {
                    monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString(), Selected = true });
                }
                else
                {
                    monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString() });
                }
            }

            string _month = months[Convert.ToInt16(month) - 1];
            int _year = years[Convert.ToInt16(year) - 1];

            DateTime selectedDate = new DateTime(_year, Convert.ToInt16(month), 1);

            if (ModelState.IsValid)
            {
                IQueryable<expl_prowizja> provisionSet =
                context.expl_prowizjaSet.Where(c => c.expl_Sprawa.Id == new Guid(caseID))
                .Where(c => c.expl_Data.Value > selectedDate)
                .Select(c => c);

                foreach (expl_prowizja elem in provisionSet)
                {
                    vm.NetSettlementList.Add(new NetSettlementObject()
                    {
                        Account = (Decimal)elem.expl_Kwota,
                        Name = new EntityReference(elem.expl_Sprawa.LogicalName, elem.expl_Sprawa.Id).Name,
                        DateOf = elem.expl_Data.Value.ToLocalTime().ToString("yyyy-MM-dd"),
                        Source = getOptionSetText("expl_prowizja", "expl_tytulem", elem.expl_Tytulem.Value)
                    });
                }

            }

            ViewBag.Client = client;
            ViewBag.Months = monthsList;
            ViewBag.Years = yearsList;
            ViewBag.CaseID = caseID;

            return View(vm);
        }

        //Rozliczenie klienta w pojedyńczej sprawie
        public ActionResult ClientSettlement(string caseID, string client)
        {

            if (null == caseID || null == client)
            {
                return HttpNotFound();
            }

            IQueryable<expl_zaliczkadlaklienta> provision =
                context.expl_zaliczkadlaklientaSet.Where(a => a.expl_Sprawa.Id.Equals(caseID));

            ClientSettlementViewModel csvm = new ClientSettlementViewModel();

            csvm.ClientName = client;

            foreach (expl_zaliczkadlaklienta item in provision)
            {
                csvm.ClientSettlementsList.Add(new ClientSettlementObject()
                {
                    Account = item.expl_Kwota.Value,
                    Case = item.expl_Sprawa.Name,
                    Name = item.expl_name,
                    ID = item.Id,
                    Type = getOptionSetText("expl_zaliczkadlaklienta", "expl_typ", item.expl_Typ.Value)
                });
            }

            ViewBag.CaseID = caseID;

            return View(csvm);
        }

        // Pozwy
        public ActionResult Summons(string caseID, string client)
        {
            if (null == caseID || null == client)
            {
                return HttpNotFound();
            }

            IQueryable<expl_pozew> summons = context.expl_pozewSet.Where(a => a.expl_sprawa.Id.Equals(new Guid(caseID)));

            SummonsViewModel svm = new SummonsViewModel();

            svm.Client = client;

            foreach (expl_pozew item in summons)
            {
                svm.list.Add(new SummonObject()
                {
                    Account = (decimal)item.expl_kwotapozwu,
                    Name = item.expl_name,
                    Guid = (Guid)item.expl_pozewId,
                    EndDate = (item.expl_zakonczeniepozwu.HasValue) ? item.expl_zakonczeniepozwu.Value.ToLocalTime().ToString("yyy-MM-dd") : "",
                    SendDate = (item.expl_wyslaniepozwu.HasValue) ? item.expl_wyslaniepozwu.Value.ToLocalTime().ToString("yyy-MM-dd") : ""
                });
            }

            ViewBag.CaseID = caseID;
            ViewBag.Client = client;

            return View(svm);
        }


        public ActionResult AdversumSettlementEntry()
        {
            return View();
        }

        #region oldAdversumSettlement

        //Get
        //Rozliczenia Adversum
        //public ActionResult AdversumSettlement(string guid, string advFunction, string full)
        //{

        //    if (null == guid || null == advFunction || null == full)
        //    {
        //        return HttpNotFound();
        //    }

        //    string polishTimeZoneName = "Central European Standard Time";

        //    TimeZoneInfo polishZone = TimeZoneInfo.FindSystemTimeZoneById(polishTimeZoneName);

        //    IQueryable<expl_prowizja> provisionSet = context.expl_prowizjaSet;

        //    foreach (expl_prowizja item in provisionSet)
        //    {
        //        item.expl_Data.Value.AddDays(1);
        //        item.expl_Data.Value.AddHours(-22);
        //    }

        //    IEnumerable<expl_prowizja> filteredResult = null;

        //    DateTime dtNow = (DateTime)GetPolishDateTimeNow();

        //    Session["polish"] = dtNow;
        //    Session["now"] = DateTime.Now.ToLocalTime();

        //    DateTime selectedDate = new DateTime(dtNow.Year, dtNow.Month, 1);
        //    DateTime selectedDateMax =
        //        new DateTime(dtNow.Year, dtNow.Month, DateTime.DaysInMonth(dtNow.Year, dtNow.Month));

        //    switch (advFunction)
        //    {
        //        case "Dyrektor Regionu":

        //            filteredResult = 
        //                provisionSet.Where(a => a.expl_DyrektorRegionu.Id == new Guid(guid))
        //                   .Where(a => a.expl_Data <= selectedDateMax)
        //                .Where(a => a.expl_Data >= selectedDate);

        //            break;
        //        case "Dyrektor Sprzedaży":

        //            filteredResult =
        //                provisionSet
        //                .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
        //                .Where(a => a.expl_Data <= selectedDateMax)
        //                .Where(a => a.expl_Data >= selectedDate);

        //            break;
        //        case "Koordynator":

        //            filteredResult =
        //                provisionSet.Where(a => a.expl_Koordynator.Id == new Guid(guid))
        //                .Where(a => a.expl_Data <= selectedDateMax)
        //                .Where(a => a.expl_Data >= selectedDate);

        //            break;
        //        case "Konsultant":

        //            filteredResult = 
        //                provisionSet.Where(a => a.expl_Konsultant.Id == new Guid(guid))
        //                .Where(a => a.expl_Data <= selectedDateMax)
        //                .Where(a => a.expl_Data >= selectedDate);

        //            break;
        //        default:
        //            break;
        //    }

        //    #region oldSwitch

        //    //switch (advFunction)
        //    //{
        //    //    case "Dyrektor Regionu":

        //    //        filteredResult =
        //    //            provisionSet.Where(a => a.expl_DyrektorRegionu.Id == new Guid(guid))
        //    //                .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //                .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());

        //    //        break;
        //    //    case "Dyrektor Sprzedaży":

        //    //        if (null != ad)
        //    //        {
        //    //            filteredResult =
        //    //            provisionSet
        //    //            //.Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
        //    //            .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
        //    //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //                .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }
        //    //        else
        //    //        {
        //    //            filteredResult =
        //    //            provisionSet.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
        //    //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //                .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }

        //    //        break;
        //    //    case "Koordynator":

        //    //        if (null != ad && null != sd)
        //    //        {
        //    //            filteredResult =
        //    //            provisionSet
        //    //            //.Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
        //    //            //.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
        //    //            .Where(a => a.expl_Koordynator.Id == new Guid(guid))
        //    //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }
        //    //        else if (null == ad && null != sd)
        //    //        {
        //    //            filteredResult =
        //    //            provisionSet
        //    //            //.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
        //    //            .Where(a => a.expl_Koordynator.Id == new Guid(guid))
        //    //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }
        //    //        else
        //    //        {
        //    //            filteredResult =
        //    //            provisionSet.Where(a => a.expl_Koordynator.Id == new Guid(guid))
        //    //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }

        //    //        break;
        //    //    case "Konsultant":

        //    //        if (null != ad && null != sd && null != cd)
        //    //        {
        //    //            filteredResult =
        //    //            provisionSet
        //    //            //.Where(a => (a.expl_DyrektorRegionu.Id == new Guid(ad)))
        //    //            //.Where(a => (a.expl_Dyrektorsprzedazy.Id == new Guid(sd)))
        //    //            //.Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
        //    //            .Where(a => a.expl_Konsultant.Id == new Guid(guid))
        //    //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }
        //    //        else if (null == ad && null != sd && null != cd)
        //    //        {
        //    //            filteredResult =
        //    //            provisionSet
        //    //            //.Where(a => (a.expl_Dyrektorsprzedazy.Id == new Guid(sd)))
        //    //            //.Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
        //    //            .Where(a => a.expl_Konsultant.Id == new Guid(guid))
        //    //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }
        //    //        else if (null == ad && null == sd && null != cd)
        //    //        {
        //    //            filteredResult =
        //    //           provisionSet
        //    //           //.Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
        //    //           .Where(a => a.expl_Konsultant.Id == new Guid(guid))
        //    //           .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //           .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }
        //    //        else
        //    //        {
        //    //            filteredResult =
        //    //           provisionSet
        //    //           .Where(a => a.expl_Konsultant.Id == new Guid(guid))
        //    //           .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
        //    //           .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
        //    //        }

        //    //        break;
        //    //    default:
        //    //        break;
        //    //}

        //    #endregion oldSwitch


        //    AdversumSettlementViewModel asvm = new AdversumSettlementViewModel();

        //    asvm.Summarize = 0;

        //    foreach (expl_prowizja item in filteredResult)
        //    {

        //        asvm.Summarize += item.expl_Kwota.Value;

        //        Guid accountGuid = Guid.Empty;

        //        asvm.AdversumSettlmentsList.Add(new AdversumSettlementObject()
        //        {
        //            Account = (Decimal)item.expl_Kwota,
        //            Case = item.expl_Sprawa != null ? item.expl_Sprawa.Name : "",
        //            DateOf = item.expl_Data.Value.ToString("yyyy-MM-dd HH:mm"),
        //            //AdvancePayment = new EntityReference("", accountGuid).
        //            Source = item.expl_Tytulem.HasValue ? getOptionSetText("expl_prowizja", "expl_tytulem", item.expl_Tytulem.Value) : "",
        //        });

        //    }

        //    List<SelectListItem> monthsList = new List<SelectListItem>();

        //    int currentYear = dtNow.Year;

        //    List<SelectListItem> yearsList = new List<SelectListItem>();

        //    for (int i = (currentYear - 5), y = 1; i <= (currentYear); i++, y++)
        //    {
        //        if (i == currentYear)
        //        {
        //            yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString(), Selected = true });
        //        }
        //        else
        //        {
        //            yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString() });
        //        }
        //    }

        //    string[] months = {
        //        "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
        //        "Lipiec" ,"Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
        //    };

        //    for (int i = 0; i < 12; i++)
        //    {
        //        if (i == (selectedDate.Month - 1))
        //        {
        //            monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString(), Selected = true });
        //        }
        //        else
        //        {
        //            monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString() });
        //        }
        //    }

        //    ViewBag.FullName = full;
        //    ViewBag.Months = monthsList;
        //    ViewBag.Years = yearsList;

        //    return View(asvm);
        //}

        #endregion oldAdversumSettlement



        public ActionResult AdversumSettlement(string guid, string advFunction, string full, string ad = null, string sd = null, string cd = null)
        {

            if (null == guid || null == advFunction || null == full)
            {
                return HttpNotFound();
            }

            string polishTimeZoneName = "Central European Standard Time";

            TimeZoneInfo polishZone = TimeZoneInfo.FindSystemTimeZoneById(polishTimeZoneName);

            IQueryable<expl_prowizja> provisionSet = context.expl_prowizjaSet;

            IEnumerable<expl_prowizja> filteredResult = null;

            DateTime dtNow = (DateTime)GetPolishDateTimeNow();

            Session["polish"] = dtNow;
            Session["now"] = DateTime.Now.ToLocalTime();

            DateTime selectedDate = new DateTime(dtNow.Year, dtNow.Month, 1);
            DateTime selectedDateMax =
                new DateTime(dtNow.Year, dtNow.Month, DateTime.DaysInMonth(dtNow.Year, dtNow.Month));

            switch (advFunction)
            {
                case "Dyrektor Regionu":

                    filteredResult =
                        provisionSet.Where(a => a.expl_DyrektorRegionu.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                    .Where(a => a.expl_Data >= selectedDate.AddHours(-2));

                    break;
                case "Dyrektor Sprzedaży":

                    if (null != ad)
                    {
                        filteredResult =
                        provisionSet
                        .Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
                        .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));

                    }
                    else
                    {
                        filteredResult =
                        provisionSet.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));

                    }


                    break;
                case "Koordynator":

                    if (null != ad && null != sd)
                    {
                        filteredResult =
                        provisionSet
                        .Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
                        .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
                        .Where(a => a.expl_Koordynator.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                    }
                    else if (null == ad && null != sd)
                    {
                        filteredResult =
                        provisionSet
                        .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
                        .Where(a => a.expl_Koordynator.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                    }
                    else
                    {
                        filteredResult =
                        provisionSet
                        .Where(a => a.expl_Koordynator.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                    }



                    break;
                case "Konsultant":

                    if (null != ad && null != sd && null != cd)
                    {
                        filteredResult =
                        provisionSet
                        .Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
                        .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
                        .Where(a => a.expl_Koordynator.Id == new Guid(cd))
                        .Where(a => a.expl_Konsultant.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                    }
                    else if (null == ad && null != sd && null != cd)
                    {
                        filteredResult =
                        provisionSet
                        .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
                        .Where(a => a.expl_Koordynator.Id == new Guid(cd))
                        .Where(a => a.expl_Konsultant.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                    }
                    else if (null == ad && null == sd && null != cd)
                    {
                        filteredResult =
                        provisionSet
                        .Where(a => a.expl_Koordynator.Id == new Guid(cd))
                        .Where(a => a.expl_Konsultant.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                    }
                    else
                    {
                        filteredResult =
                        provisionSet.Where(a => a.expl_Konsultant.Id == new Guid(guid))
                        .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                    }


                    break;
                default:
                    break;
            }

            #region oldSwitch

            //switch (advFunction)
            //{
            //    case "Dyrektor Regionu":

            //        filteredResult =
            //            provisionSet.Where(a => a.expl_DyrektorRegionu.Id == new Guid(guid))
            //                .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //                .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());

            //        break;
            //    case "Dyrektor Sprzedaży":

            //        if (null != ad)
            //        {
            //            filteredResult =
            //            provisionSet
            //            //.Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
            //            .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
            //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //                .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }
            //        else
            //        {
            //            filteredResult =
            //            provisionSet.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
            //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //                .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }

            //        break;
            //    case "Koordynator":

            //        if (null != ad && null != sd)
            //        {
            //            filteredResult =
            //            provisionSet
            //            //.Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
            //            //.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
            //            .Where(a => a.expl_Koordynator.Id == new Guid(guid))
            //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }
            //        else if (null == ad && null != sd)
            //        {
            //            filteredResult =
            //            provisionSet
            //            //.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
            //            .Where(a => a.expl_Koordynator.Id == new Guid(guid))
            //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }
            //        else
            //        {
            //            filteredResult =
            //            provisionSet.Where(a => a.expl_Koordynator.Id == new Guid(guid))
            //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }

            //        break;
            //    case "Konsultant":

            //        if (null != ad && null != sd && null != cd)
            //        {
            //            filteredResult =
            //            provisionSet
            //            //.Where(a => (a.expl_DyrektorRegionu.Id == new Guid(ad)))
            //            //.Where(a => (a.expl_Dyrektorsprzedazy.Id == new Guid(sd)))
            //            //.Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
            //            .Where(a => a.expl_Konsultant.Id == new Guid(guid))
            //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }
            //        else if (null == ad && null != sd && null != cd)
            //        {
            //            filteredResult =
            //            provisionSet
            //            //.Where(a => (a.expl_Dyrektorsprzedazy.Id == new Guid(sd)))
            //            //.Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
            //            .Where(a => a.expl_Konsultant.Id == new Guid(guid))
            //            .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //            .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }
            //        else if (null == ad && null == sd && null != cd)
            //        {
            //            filteredResult =
            //           provisionSet
            //           //.Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
            //           .Where(a => a.expl_Konsultant.Id == new Guid(guid))
            //           .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //           .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }
            //        else
            //        {
            //            filteredResult =
            //           provisionSet
            //           .Where(a => a.expl_Konsultant.Id == new Guid(guid))
            //           .Where(a => a.expl_Data <= selectedDateMax.ToUniversalTime())
            //           .Where(a => a.expl_Data >= selectedDate.ToUniversalTime());
            //        }

            //        break;
            //    default:
            //        break;
            //}

            #endregion oldSwitch


            AdversumSettlementViewModel asvm = new AdversumSettlementViewModel();

            asvm.Summarize = 0;

            foreach (expl_prowizja item in filteredResult)
            {

                asvm.Summarize += item.expl_Kwota.Value;

                Guid accountGuid = Guid.Empty;
                string caseName = "";

                if (item.expl_ProwizjaRozliczana != null)
                {
                    var zaliczka = context.expl_prowizjaSet
                    .Where(c => ((c.Id == item.expl_ProwizjaRozliczana.Id)));

                    foreach (expl_prowizja b in zaliczka)
                    {
                        caseName = b.expl_Sprawa.Name.ToString();
                    }

                }

                asvm.AdversumSettlmentsList.Add(new AdversumSettlementObject()
                {
                    Account = (Decimal)item.expl_Kwota,
                    Case = item.expl_Sprawa != null ? item.expl_Sprawa.Name : "",
                    DateOf = item.expl_Data.Value.AddHours(2).ToString("yyyy-MM-dd"),
                    AdvancePayment = caseName,
                    Source = item.expl_Tytulem.HasValue ? getOptionSetText("expl_prowizja", "expl_tytulem", item.expl_Tytulem.Value) : "",
                });

            }

            List<SelectListItem> monthsList = new List<SelectListItem>();

            int currentYear = dtNow.Year;

            List<SelectListItem> yearsList = new List<SelectListItem>();

            for (int i = (currentYear - 5), y = 1; i <= (currentYear); i++, y++)
            {
                if (i == currentYear)
                {
                    yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString(), Selected = true });
                }
                else
                {
                    yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString() });
                }
            }

            string[] months = {
                "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
                "Lipiec" ,"Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
            };

            for (int i = 0; i < 12; i++)
            {
                if (i == (selectedDate.Month - 1))
                {
                    monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString(), Selected = true });
                }
                else
                {
                    monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString() });
                }
            }

            ViewBag.FullName = full;
            ViewBag.Months = monthsList;
            ViewBag.Years = yearsList;

            return View(asvm);
        }


        // Rozliczenia Adversum
        [HttpPost]
        public ActionResult AdversumSettlement(string guid, string advFunction, string Months, string Years,
            AdversumSettlementViewModel asvm, string full, string ad = null, string sd = null, string cd = null)
        {

            IQueryable<expl_prowizja> provisionSet = context.expl_prowizjaSet;

            IEnumerable<expl_prowizja> filteredResult = null;

            List<SelectListItem> monthsList = new List<SelectListItem>();

            int currentYear = DateTime.Now.Year;

            int[] yearsArr = new int[6];

            for (int i = (currentYear - 5), y = 0; i <= (currentYear); i++, y++)
            {
                yearsArr[y] = i;
            }

            string[] months = {
                "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
                "Lipiec" ,"Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
            };

            DateTime selectedDate = new DateTime(yearsArr[Convert.ToInt16(Years) - 1], Convert.ToInt16(Months), 1, 0, 0, 0);

            DateTime selectedDateMax =
                new DateTime(selectedDate.Year, selectedDate.Month, DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month), 23, 59, 59);


            List<SelectListItem> yearsList = new List<SelectListItem>();

            for (int i = (currentYear - 5), y = 1; i <= (currentYear); i++, y++)
            {
                if (i == selectedDate.Year)
                {
                    yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString(), Selected = true });
                }
                else
                {
                    yearsList.Add(new SelectListItem() { Text = i.ToString(), Value = y.ToString() });
                }
            }



            for (int i = 0; i < 12; i++)
            {
                if (i == (selectedDate.Month - 1))
                {
                    monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString(), Selected = true });
                }
                else
                {
                    monthsList.Add(new SelectListItem() { Text = months[i], Value = (i + 1).ToString() });
                }
            }


            if (ModelState.IsValid)
            {

                switch (advFunction)
                {
                    case "Dyrektor Regionu":

                        filteredResult =
                            provisionSet.Where(a => a.expl_DyrektorRegionu.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                        .Where(a => a.expl_Data >= selectedDate.AddHours(-2));

                        break;
                    case "Dyrektor Sprzedaży":

                        if (null != ad)
                        {
                            filteredResult =
                            provisionSet
                            .Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
                            .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));

                        }
                        else
                        {
                            filteredResult =
                            provisionSet.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));

                        }


                        break;
                    case "Koordynator":

                        if (null != ad && null != sd)
                        {
                            filteredResult =
                            provisionSet
                            .Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
                            .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
                            .Where(a => a.expl_Koordynator.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                        }
                        else if (null == ad && null != sd)
                        {
                            filteredResult =
                            provisionSet
                            .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
                            .Where(a => a.expl_Koordynator.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                        }
                        else
                        {
                            filteredResult =
                            provisionSet
                            .Where(a => a.expl_Koordynator.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                        }



                        break;
                    case "Konsultant":

                        if (null != ad && null != sd && null != cd)
                        {
                            filteredResult =
                            provisionSet
                            .Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
                            .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
                            .Where(a => a.expl_Koordynator.Id == new Guid(cd))
                            .Where(a => a.expl_Konsultant.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                        }
                        else if (null == ad && null != sd && null != cd)
                        {
                            filteredResult =
                            provisionSet
                            .Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(sd))
                            .Where(a => a.expl_Koordynator.Id == new Guid(cd))
                            .Where(a => a.expl_Konsultant.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                        }
                        else if (null == ad && null == sd && null != cd)
                        {
                            filteredResult =
                            provisionSet
                            .Where(a => a.expl_Koordynator.Id == new Guid(cd))
                            .Where(a => a.expl_Konsultant.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                        }
                        else
                        {
                            filteredResult =
                            provisionSet.Where(a => a.expl_Konsultant.Id == new Guid(guid))
                            .Where(a => a.expl_Data <= selectedDateMax.AddHours(-2))
                            .Where(a => a.expl_Data >= selectedDate.AddHours(-2));
                        }


                        break;
                    default:
                        break;
                }

                asvm.Summarize = 0;

                foreach (expl_prowizja item in filteredResult)
                {

                    string caseName = "";

                    if (item.expl_ProwizjaRozliczana != null)
                    {
                        var zaliczka = context.expl_prowizjaSet
                        .Where(c => ((c.Id == item.expl_ProwizjaRozliczana.Id)));

                        foreach (expl_prowizja b in zaliczka)
                        {
                            caseName = b.expl_Sprawa.Name.ToString();
                        }

                    }

                    asvm.Summarize += item.expl_Kwota.Value;

                    asvm.AdversumSettlmentsList.Add(new AdversumSettlementObject()
                    {
                        Account = (Decimal)item.expl_Kwota,
                        Case = item.expl_Sprawa != null ? item.expl_Sprawa.Name : "",
                        DateOf = item.expl_Data.Value.AddHours(2).ToString("yyyy-MM-dd"),
                        AdvancePayment = caseName,
                        Source = item.expl_Tytulem.HasValue ? getOptionSetText("expl_prowizja", "expl_tytulem", item.expl_Tytulem.Value) : "",
                    });
                }
            }

            ViewBag.FullName = full;
            ViewBag.Months = monthsList;
            ViewBag.Years = yearsList;

            return View(asvm);
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
                var login = "supportexisto@adversum.onmicrosoft.com";
                var password = "AdvCRM2013!";

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
            var login = "supportexisto@adversum.onmicrosoft.com";
            var password = "AdvCRM2013!";

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