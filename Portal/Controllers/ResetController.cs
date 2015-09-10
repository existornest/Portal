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
using System.Net.Mail;
using Portal.Library;

namespace Portal.Controllers
{
    public class ResetController : Controller
    {
        private ResetContext db = new ResetContext();
        protected XrmServiceContext context = new ConnectionContext().XrmContext;

        //// GET: Resets
        //public ActionResult Index()
        //{
        //    return View(db.ResetModelContext.ToList());
        //}

        // GET: Resets/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Reset reset = db.ResetModelContext.Find(id);
        //    if (reset == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(reset);
        //}

        // GET: Reset/Index
        public ActionResult Index()
        {
            return View();
        }

        // POST: Reset/Index
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "ID,UserName,Email,Password,ConfirmPassword")] Reset reset)
        {
            if (ModelState.IsValid)
            {
                Contact user = context.ContactSet
                    .Where(a => a.expl_PortalLogin == reset.UserName)
                    .Select(row => row).FirstOrDefault();

                if (null == user)
                {
                    TempData["loginError"] = "Nie ma takiego użytkownika.";
                    Session["loggedUser"] = null;
                    return RedirectToAction("Index", "Home");
                }

                PasswordHash pHash = PasswordHash.Create(reset.Password);

                string emailGuid = (context.ContactSet
                    .Where(a => a.expl_PortalLogin == reset.UserName)
                    .Select(row => row.ContactId).FirstOrDefault()).ToString();

                Session[emailGuid] = reset.Password;
                Session[emailGuid + "_hash"] = pHash.Hash;
                Session[emailGuid + "_salt"] = pHash.Salt;

                string link = "<a href='http://localhost:60774/Reset/ResetPassword" + "?id=" +
                    emailGuid + "'>Resetuj hasło</a>";

                try
                {

                    
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(reset.Email));
                    message.From = new MailAddress("rnest@rnest.prohost.pl");
                    message.Subject = "Reset hasła";
                    message.Body = "Link do resetu hasła: " + link;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "rnest@rnest.prohost.pl",
                            Password = "rtec4444"
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "pol51.nameserverus2.com";
                        smtp.Port = 25;
                        smtp.EnableSsl = false;
                        smtp.Send(message);

                        TempData["info"] = "Potwierdzajacy email został wysłany na podany adres email.";
                        return RedirectToAction("Index", "Home");
                    }

                }
                catch(Exception e)
                {
                    TempData["loginError"] = "Wystąpił błąd. Skontaktuj się z administracją.";
                    return RedirectToAction("Index", "Home");
                }


            }

            return View(reset);
        }


        public ActionResult ResetPassword(string id)
        {

            if (null == id)
            {
                return HttpNotFound();
            }

            

            try
            {
                Contact contact = (Contact)
                    context.Retrieve("contact", new Guid(id), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                contact.expl_Haslo = (string)Session[id];
                contact.expl_salt = (string)Session[id + "_salt"];
                contact.expl_passwordhash = (string)Session[id + "_hash"];

                context.Update(contact);

                context.SaveChanges();

                Session[id] = null;
                Session[id + "_salt"] = null;
                Session[id + "_hash"] = null;


            }
            catch(Exception e)
            {
                Session[id] = null;
                Session[id + "_salt"] = null;
                Session[id + "_hash"] = null;

                TempData["loginError"] = "Wystąpił błąd. Skontaktuj się z Administracją.";
                return RedirectToAction("Index", "Home");
            }

            Session[id] = null;
            Session[id + "_salt"] = null;
            Session[id + "_hash"] = null;

            TempData["info"] = "Hasło zostało zmienione.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Resets/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Reset reset = db.ResetModelContext.Find(id);
        //    if (reset == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(reset);
        //}

        // POST: Resets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "ID,UserName,Password,ConfirmPassword")] Reset reset)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(reset).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(reset);
        //}

        // GET: Resets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reset reset = db.ResetModelContext.Find(id);
            if (reset == null)
            {
                return HttpNotFound();
            }
            return View(reset);
        }

        // POST: Resets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reset reset = db.ResetModelContext.Find(id);
            db.ResetModelContext.Remove(reset);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
