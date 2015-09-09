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
        private LoginModelContext db = new LoginModelContext();
        protected XrmServiceContext context = new ConnectionContext().XrmContext;

        //// GET: LoginModels
        //public ActionResult Index()
        //{
        //    return View(db.LoginModels.ToList());
        //}

        // GET: LoginModels/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoginModel loginModel = db.LoginModels.Find(id);
            if (loginModel == null)
            {
                return HttpNotFound();
            }
            return View(loginModel);
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

                    //try
                    //{
                        
                    //}
                    //catch
                    //{
                    //    TempData["loginError"] = "Błędy logowania.";
                    //    Session.RemoveAll();
                    //    return RedirectToAction("Index");
                    //}

                    if (check != "")
                    {
                        Session["netUser"] = 1;
                        TempData["info"] = "Logowanie poprawne.";
                        return RedirectToAction("Index", "PortalNet");
                    }


                    //Debugger.Break();

                    Session["netUser"] = 0;
                    TempData["info"] = "Logowanie poprawne.";
                    return RedirectToAction("Index", "Portal");
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
            TempData["info"] = "Wylogowano poprawnie.";
            return RedirectToAction("Index", "Login");
        }

        // GET: LoginModels/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoginModel loginModel = db.LoginModels.Find(id);
            if (loginModel == null)
            {
                return HttpNotFound();
            }
            return View(loginModel);
        }

        // POST: LoginModels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserName,Password")] LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(loginModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(loginModel);
        }

        // GET: LoginModels/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoginModel loginModel = db.LoginModels.Find(id);
            if (loginModel == null)
            {
                return HttpNotFound();
            }
            return View(loginModel);
        }

        // POST: LoginModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LoginModel loginModel = db.LoginModels.Find(id);
            db.LoginModels.Remove(loginModel);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
