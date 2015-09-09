using Portal.Library.Controllers;
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

            //Debugger.Break();

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
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    //Session["AD"] = guid;
                    break;
                case "Dyrektor Sprzedaży":

                    if (null != ad)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => a.expl_DyrektorRegionu.Id == new Guid(ad))
                        .Where(a => a.expl_DyrSprzedazy.Id == new Guid(guid))
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => a.expl_DyrSprzedazy.Id == new Guid(guid))
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    //Session["SD"] = guid;
                    break;
                case "Koordynator":

                    if (null != ad && null != sd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_DyrektorRegionu.Id == new Guid(ad)))
                        .Where(a => (a.expl_DyrSprzedazy.Id == new Guid(sd)))
                        .Where(a => (a.expl_Koordynator.Id == new Guid(guid)))
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else if (null == ad && null != sd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_DyrSprzedazy.Id == new Guid(sd)))
                        .Where(a => (a.expl_Koordynator.Id == new Guid(guid)))
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_Koordynator.Id == new Guid(guid)))
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    //Session["CD"] = guid;
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
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else if (null == ad && null != sd && null != cd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_DyrSprzedazy.Id == new Guid(sd)))
                        .Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
                        .Where(a => (a.expl_Konsultant.Id == new Guid(guid)))
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else if (null == ad && null == sd && null != cd)
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_Koordynator.Id == new Guid(cd)))
                        .Where(a => (a.expl_Konsultant.Id == new Guid(guid)))
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    else
                    {
                        tickets = context
                        .IncidentSet
                        .Where(a => (a.expl_Konsultant.Id == new Guid(guid)))
                        .OrderBy(a => a.expl_Ostatniedzialanie)
                        .Select(a => a);
                    }
                    //Session["CO"] = guid;
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
            cvm.Kontakt = incident.CustomerId.Name;
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

            //Debugger.Break();
            ViewBag.Client = client;
            ViewBag.Months = monthsList;
            ViewBag.Years = yearsList;

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

            return View(svm);
        }


        public ActionResult AdversumSettlementEntry()
        {
            return View();
        }


        //Get
        //Rozliczenia Adversum
        public ActionResult AdversumSettlement(string guid, string advFunction, string full)
        {

            if (null == guid || null == advFunction || null == full)
            {
                return HttpNotFound();
            }

            IQueryable<expl_prowizja> provisionSet = context.expl_prowizjaSet;
            IQueryable<expl_prowizja> filteredResult = null;

            DateTime selectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            switch (advFunction)
            {
                case "Dyrektor Regionu":

                    filteredResult =
                        provisionSet.Where(a => a.expl_DyrektorRegionu.Id == new Guid(guid))
                        //.Where(a => a.expl_Kontakt.Id == (Guid)Session["guid"])
                        .Where(a => a.expl_Data.Value > selectedDate);

                    break;
                case "Dyrektor Sprzedaży":

                    filteredResult =
                        provisionSet.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
                        //.Where(a => a.expl_Kontakt.Id == (Guid)Session["guid"])
                        .Where(a => a.expl_Data.Value > selectedDate);

                    break;
                case "Koordynator":

                    filteredResult =
                        provisionSet.Where(a => a.expl_Koordynator.Id == new Guid(guid))
                        //.Where(a => a.expl_Kontakt.Id == (Guid)Session["guid"])
                        .Where(a => a.expl_Data.Value > selectedDate);

                    break;
                case "Konsultant":

                    filteredResult =
                        provisionSet.Where(a => a.expl_Konsultant.Id == new Guid(guid))
                        //.Where(a => a.expl_Kontakt.Id == (Guid)Session["guid"])
                        .Where(a => a.expl_Data.Value > selectedDate);

                    break;
                default:
                    break;
            }

            // if (a.expl_ProwizjaRozliczana != null)
            // {
            //     var zaliczka = xrm.expl_prowizjaSet
            //.Where(c => ((c.Id == a.expl_ProwizjaRozliczana.Id)));

            //     foreach (expl_prowizja b in zaliczka)
            //     {
            //         cell.Text = b.expl_Sprawa.Name.ToString();
            //     }

            // }
            // else
            // {
            //     cell.Text = "";
            // }

            AdversumSettlementViewModel asvm = new AdversumSettlementViewModel();

            asvm.Summarize = 0;

            foreach (expl_prowizja item in filteredResult)
            {

                asvm.Summarize += item.expl_Kwota.Value;

                asvm.AdversumSettlmentsList.Add(new AdversumSettlementObject()
                {
                    Account = (Decimal)item.expl_Kwota,
                    Case = item.expl_Sprawa != null ? item.expl_Sprawa.Name : "",
                    DateOf = item.expl_Data.Value.ToLocalTime().ToString("yyyy-MM-dd"),
                    //AdvancePayment = item.expl_ProwizjaRozliczana != null ? new EntityReference("expl_zaliczka", "expl_sprawa", item.expl_zaliczka.Id).Name : "",
                    Source = item.expl_Tytulem.HasValue ? getOptionSetText("expl_prowizja", "expl_tytulem", item.expl_Tytulem.Value) : "",
                });

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
            AdversumSettlementViewModel asvm, string full)
        {

            IQueryable<expl_prowizja> provisionSet = context.expl_prowizjaSet;
            IQueryable<expl_prowizja> filteredResult = null;

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

            DateTime selectedDate = new DateTime(yearsArr[Convert.ToInt16(Years) - 1], Convert.ToInt16(Months), 1);

            //int daysNum = DateTime.DaysInMonth(yearsArr[Convert.ToInt16(Years) - 1], (Convert.ToInt16(Months) + 1));
            //int monthNum = (Convert.ToInt16(Months) + 1);
            //int yearNum = yearsArr[Convert.ToInt16(Years) - 1];

            DateTime selectedDateMax = 
                new DateTime(selectedDate.Year, selectedDate.Month, DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month));

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
                            .Where(a => a.expl_Data.Value < selectedDateMax)
                            .Where(a => a.expl_Data.Value > selectedDate);

                        break;
                    case "Dyrektor Sprzedaży":

                        filteredResult =
                            provisionSet.Where(a => a.expl_Dyrektorsprzedazy.Id == new Guid(guid))
                            .Where(a => a.expl_Data.Value < selectedDateMax)
                            .Where(a => a.expl_Data.Value > selectedDate);

                        break;
                    case "Koordynator":

                        filteredResult =
                            provisionSet.Where(a => a.expl_Koordynator.Id == new Guid(guid))
                            .Where(a => a.expl_Data.Value < selectedDateMax)
                            .Where(a => a.expl_Data.Value > selectedDate);

                        break;
                    case "Konsultant":

                        filteredResult =
                            provisionSet.Where(a => a.expl_Konsultant.Id == new Guid(guid))
                            .Where(a => a.expl_Data.Value < selectedDateMax)
                            .Where(a => a.expl_Data.Value > selectedDate);

                        break;
                    default:
                        break;
                }

                asvm.Summarize = 0;

                foreach (expl_prowizja item in filteredResult)
                {

                    asvm.Summarize += item.expl_Kwota.Value;

                    asvm.AdversumSettlmentsList.Add(new AdversumSettlementObject()
                    {
                        Account = (Decimal)item.expl_Kwota,
                        Case = item.expl_Sprawa != null ? item.expl_Sprawa.Name : "",
                        DateOf = item.expl_Data.Value.ToLocalTime().ToString("yyyy-MM-dd"),
                        AdvancePayment = item.expl_ProwizjaRozliczana != null ? new EntityReference("expl_prowizjarozliczana", item.expl_ProwizjaRozliczana.Id).Name : "",
                        Source = item.expl_Tytulem.HasValue ? getOptionSetText("expl_prowizja", "expl_tytulem", item.expl_Tytulem.Value) : "",
                    });
                }
            }

            ViewBag.FullName = full;
            ViewBag.Months = monthsList;
            ViewBag.Years = yearsList;

            return View(asvm);
        }


    }
}