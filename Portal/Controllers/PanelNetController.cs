using EarlyBoundTypes;
using Portal.Library.Controllers;
using Portal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class PanelNetController : CommonController
    {
        // GET: Panel
        public ActionResult Index(string guid)
        {
            if (null == guid)
            {
                return HttpNotFound();
            }

            Contact contact = context.ContactSet.Where(a => a.ContactId == new Guid(guid)).FirstOrDefault();

            PanelViewModel pvm = new PanelViewModel();

            pvm.Guid = guid;
            pvm.Email = contact.EMailAddress1;
            pvm.FullName = contact.FullName;
            pvm.Address = contact.Address1_Composite != null ? contact.Address1_Composite : "";
            pvm.PIN = contact.expl_PESEL != null ? contact.expl_PESEL : "";

            return View(pvm);
        }
    }
}