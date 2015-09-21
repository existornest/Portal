using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using EarlyBoundTypes;
using PortalCRM.Library;
using Portal.Library;
using Portal.Library.Tree;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;

namespace Portal.Controllers
{
    public class LoginController : Controller
    {

        protected XrmServiceContext context = null;

        public LoginController(): base()
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

        // GET: Login/Index
        public ActionResult Index()
        {
            

            return View();
        }

        // POST: Login/Index
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "ID,UserName,Password")] LoginModel loginModel)
        {

            if (ModelState.IsValid)
            {

                Contact user = context.ContactSet
                    .Where(a => a.expl_PortalLogin.Equals(loginModel.UserName))
                    .Select(row => row).FirstOrDefault();

                if (null == user)
                {
                    TempData["loginError"] = "Nie ma takiego użytkownika.";
                    return RedirectToAction("Index");
                }


                if (null == user.expl_passwordhash)
                {
                    TempData["loginError"] = "Użytkownik nie posiada uprawnień do logowania do Portalu.";
                    return RedirectToAction("Index");
                }

                PasswordHash pHash = PasswordHash.Create(user.expl_salt, user.expl_passwordhash);

                if (pHash.Verify(loginModel.Password))
                {
                    Session["loggedUser"] = loginModel.UserName;
                    Session["guid"] = user.ContactId;
                    Session["userName"] = user.FullName;

                    string check = "";

                    CreateTree ct = new CreateTree(context, (Guid)user.ContactId);
                    Session["tree"] = check = ct.Html;

                    CreateTreeAdversumSettlement ctas = new CreateTreeAdversumSettlement(context, (Guid)user.ContactId);
                    Session["treeAS"] = ctas.Html;

                   
                    if (check != "")
                    {
                        Session["netUser"] = 1;
                       
                    }
                    else
                    {
                        Session["netUser"] = 0;
                    }


                    return RedirectToAction("AccountOrContact", "Login");
                }

                TempData["loginError"] = "Błędne hasło.";
                return RedirectToAction("Index");
            }

            return View(loginModel);
        }

        public ActionResult LogOut()
        {
            Session["loggedUser"] = null;
            Session["guid"] = null;
            Session.RemoveAll();
            
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AccountOrContact()
        {

            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem() { Text = "Jako Firma", Value = "0" });
            list.Add(new SelectListItem() { Text = "Jako Osoba Fizyczna", Value = "1", Selected = true });

            ViewBag.AccountOrContact = list;

            return View();
        }

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
