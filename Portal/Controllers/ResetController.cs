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
using System.Diagnostics;
using System.Configuration;

namespace Portal.Controllers
{
    public class ResetController : Controller
    {
        //private ResetContext db = new ResetContext();
        protected XrmServiceContext context = new ConnectionContext().XrmContext;

        public string BaseURL()
        {
            return GetBaseUrl();
        }

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
        public ActionResult Index([Bind(Include = "ID,UserName,OldPassword,Password,ConfirmPassword")] Reset reset)
        {
            if (ModelState.IsValid)
            {
                Contact user = context.ContactSet
                    .Where(a => a.expl_PortalLogin == reset.UserName)
                    .Select(row => row).FirstOrDefault();

                string email = user.EMailAddress1;
                

                if (null == email)
                {
                    Session.RemoveAll();
                    TempData["loginError"] = "Użytkownik nie posiada przypisanego adresu email w systemie CRM.";
                    Session["loggedUser"] = null;
                    return RedirectToAction("Index", "Login");
                }

                if (null == user)
                {
                    Session.RemoveAll();
                    TempData["loginError"] = "Nie ma takiego użytkownika.";
                    Session["loggedUser"] = null;
                    return RedirectToAction("Index", "Login");
                }

                PasswordHash pHash = PasswordHash.Create(reset.Password);
                PasswordHash pVerify = null;

                try
                {
                    pVerify = PasswordHash.Create(user.expl_salt, user.expl_passwordhash);

                }
                catch
                {
                    Session.RemoveAll();
                    TempData["loginError"] = "Użytkownik nie może w tej chwili resetować hasła.";
                    Session["loggedUser"] = null;
                    return RedirectToAction("Index", "Login");
                }
                
                if(!pVerify.Verify(reset.OldPassword))
                {
                    Session.RemoveAll();
                    TempData["loginError"] = "Wpisz poprawnie stare hasło.";
                    Session["loggedUser"] = null;
                    return RedirectToAction("Index", "Reset");
                }

                string emailGuid = (context.ContactSet
                    .Where(a => a.expl_PortalLogin == reset.UserName)
                    .Select(row => row.ContactId).FirstOrDefault()).ToString();

                Session[emailGuid] = reset.Password;
                Session[emailGuid + "_hash"] = pHash.Hash;
                Session[emailGuid + "_salt"] = pHash.Salt;

                //string link = "<a href='http://localhost:60774/Reset/ResetPassword" + "?id=" +
                //    emailGuid + "'>Resetuj hasło</a>";

                string link = "<a href='" + GetBaseUrl() + "Reset/ResetPassword" + "?id=" +
                    emailGuid + "'>Resetuj hasło</a>";


                try
                {
                   

                    var message = new MailMessage();
                    message.To.Add(new MailAddress(email));
                    message.From = new MailAddress(ConfigurationManager.AppSettings["email"]);
                    message.Subject = "Reset hasła";
                    message.Body = "Link do resetu hasła: " + link;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = ConfigurationManager.AppSettings["email_username"],
                            Password = ConfigurationManager.AppSettings["email_password"]
                        };
                        smtp.Credentials = credential;
                        smtp.Host = ConfigurationManager.AppSettings["email_host"];
                        smtp.Port = Convert.ToInt16(ConfigurationManager.AppSettings["email_smtp_port"]);
                        smtp.EnableSsl = false;
                        smtp.Send(message);
                        
                        TempData["info"] = "Potwierdzajacy email został wysłany na podany adres email.";
                        return RedirectToAction("Index", "Login");
                    }

                }
                catch(Exception e)
                {
                    Session.RemoveAll();
                    TempData["loginError"] = "Wystąpił błąd. Skontaktuj się z administracją.";
                    return RedirectToAction("Index", "Login");
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
                Session.RemoveAll();

            }
            catch(Exception e)
            {
                Session[id] = null;
                Session[id + "_salt"] = null;
                Session[id + "_hash"] = null;
                Session.RemoveAll();
                TempData["loginError"] = "Wystąpił błąd. Skontaktuj się z Administracją.";
                return RedirectToAction("Index", "Login");
            }

            Session[id] = null;
            Session[id + "_salt"] = null;
            Session[id + "_hash"] = null;


            Session.RemoveAll();
            TempData["info"] = "Hasło zostało zmienione.";
            return RedirectToAction("Index", "Login");
        }

        public string GetBaseUrl()
        {
            var request = HttpContext.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            //if (!string.IsNullOrWhiteSpace(appUrl)) appUrl += "/";

            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);
            return baseUrl;
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
