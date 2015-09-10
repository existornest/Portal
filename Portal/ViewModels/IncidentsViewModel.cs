using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.ViewModels
{
    public class IncidentsViewModel
    {
        public string IncidentID { get; set; }
        public string Client { get; set; }
        public string CaseNumber { get; set; }
        public string State { get; set; }
        public string DateOfCreation { get; set; }
    }
}