using EarlyBoundTypes;
using Portal.Library.Controllers;
using Portal.Library.Tree;
using PortalCRM.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class TreeTestController : CommonController
    {

        // GET: TreeTest
        public ActionResult Index()
        {

           //CreateTree ct = new CreateTree(context);
           // ViewBag.Html = ct.Render();

            //ViewBag.Html = Session["tree"];
            

            return View();
        }
    }
}