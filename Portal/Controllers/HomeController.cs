using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem() { Text = "Jako Firma", Value = "0" });
            list.Add(new SelectListItem() { Text = "Jako Osoba Fizyczna", Value = "1", Selected = true });

            ViewBag.AccountOrContact = list;

            return View();
        }

        [HttpPost]
        public ActionResult CreateSession(string accountOrContact = "2")
        {


            if (0 == Convert.ToInt16(accountOrContact))
            {
                Session["isContact"] = 0;
            }
            else if (1 == Convert.ToInt16(accountOrContact))
            {
                Session["isContact"] = 1;
            }
            else
            {
                TempData["loginError"] = "Błędy logowania. Skontaktuj się z Administracją.";
                return RedirectToAction("Index", "Home");
            }


            return RedirectToAction("Index", "Login");
        }

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}