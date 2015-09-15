using EarlyBoundTypes;
using Portal.Library;
using Portal.Models;
using Portal.ViewModels;
using PortalCRM.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class RegisterController : Controller
    {

        private XrmServiceContext context = new ConnectionContext().XrmContext;

        // GET: Register
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult DoRegistration([Bind(Include = "ID,UserName,Email,Password,ConfirmPassword")] Register reset)
        {
            if (ModelState.IsValid)
            {
                Contact user = context.ContactSet
                    .Where(a => a.expl_PortalLogin == reset.UserName)
                    .Select(row => row).FirstOrDefault();

                if (null == user)
                {
                    Session.RemoveAll();
                    TempData["loginError"] = "Nie ma takiego użytkownika.";
                    Session["loggedUser"] = null;
                    return RedirectToAction("Index", "Login");
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
                    message.From = new MailAddress("");
                    message.Subject = "Reset hasła";
                    message.Body = "Link do resetu hasła: " + link;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "",
                            Password = ""
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "";
                        smtp.Port = 25;
                        smtp.EnableSsl = false;
                        smtp.Send(message);

                        Session.RemoveAll();
                        TempData["info"] = "Potwierdzajacy email został wysłany na podany adres email.";
                        return RedirectToAction("Index", "Login");
                    }

                }
                catch (Exception e)
                {
                    Session.RemoveAll();
                    TempData["loginError"] = "Wystąpił błąd. Skontaktuj się z administracją.";
                    return RedirectToAction("Index", "Login");
                }


            }

            return View(reset);

        }

    }
}