using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Portal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            string lastError = "\n======================================\n";
            lastError += DateTime.Now.ToUniversalTime().ToString() + "\n";
            lastError += Server.GetLastError().Message + "\n";
            lastError += Server.GetLastError().Source;
            lastError += "\n======================================\n";
            string path = HostingEnvironment.ApplicationPhysicalPath + "\\Files\\log.txt";

            System.IO.StreamWriter file = new System.IO.StreamWriter(path, true);
            file.WriteLine(lastError);

            file.Close();
        }



    }
}
