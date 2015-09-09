using EarlyBoundTypes;
using PortalCRM.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Library.Controllers.Abstract
{
    public abstract class ABaseController : Controller
    {

        protected XrmServiceContext context = null;

        public ABaseController() : base()
        {
            try
            {
                context = new ConnectionContext().XrmContext;
            }
            catch (Exception e)
            {
                TempData["loginError"] = "Serwis chwilowo niedostępny.";
                Session["loggedUser"] = null;
                Session.RemoveAll();
                RedirectToAction("Index", "Home");
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            
        }

       
    }
}