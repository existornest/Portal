using EarlyBoundTypes;
using Portal.Library.Controllers;
using PortalCRM.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class HomeController : Controller
    {

        private XrmServiceContext context = null;

        public HomeController(): base()
        {

            try
            {
                context = new ConnectionContext().XrmContext;
            }
            catch
            {
                Session.RemoveAll();
                TempData["loginError"] = "Wystąpiły problemy z połaczeniem do CRM. Proszę spróbować później.";
                RedirectToAction("Index", "Login");
            }


        }


        public ActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public ActionResult CreateSession(string accountOrContact = "2")
        {

            if (1 == Convert.ToInt16(accountOrContact))
            {
                Session["isContact"] = 1;

                if(1 == (int)Session["netUser"])
                {
                    return RedirectToAction("Index", "PortalNet");
                }
                else
                {
                    return RedirectToAction("Index", "Portal");
                }
                
            }
            else if (0 == Convert.ToInt16(accountOrContact))
            {
                Session["isContact"] = 0;
                return RedirectToAction("Account", "Home");
            }
            else
            {
                Session.RemoveAll();
                TempData["loginError"] = "Błędy logowania. Skontaktuj się z Administracją.";
                return RedirectToAction("Index", "Login");
            }

            
               
        }

        //Get
        public ActionResult Account()
        {
            Guid userGuid = (Guid)Session["guid"];

            List<Account> accounts = context.AccountSet.Where(a => a.PrimaryContactId.Id == userGuid).Select(a => a).ToList();

            if(accounts.Count == 0)
            {
                if (1 == (int)Session["netUser"])
                {
                    return RedirectToAction("Index", "PortalNet");
                }
                else
                {
                    return RedirectToAction("Index", "Portal");
                }
            }

            List<SelectListItem> accountsList = new List<SelectListItem>();

            int count = 1;
            foreach (Account item in accounts)
            {
                accountsList.Add(new SelectListItem() { Text = item.Name, Value = item.AccountId.ToString(), Selected = (count == 1)? true : false});
                count++;
            }

            ViewBag.Accounts = accountsList;

            return View();
        }

        [HttpPost]
        public ActionResult Account(string Accounts)
        {
            if(null == Accounts)
            {
                return HttpNotFound();
            }

            Guid userGuid = (Guid)Session["guid"];

            Account account = context.AccountSet.Where(a => a.Id == new Guid(Accounts)).Select(a => a).FirstOrDefault();
            Session["account"] = account;

            if (1 == (int)Session["netUser"])
            {
                return RedirectToAction("Index", "PortalNet");
            }
            else
            {
                return RedirectToAction("Index", "Portal");
            }

            
        }


    }
}