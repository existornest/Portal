using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class IncidentObject
    {
        public Guid ID { get; set; }
        public string Client { get; set; }
        public string Ticket { get; set; }
        public string State { get; set; }
        public string LastActionDate { get; set; }
        public string DateCreated { get; set; }
    }

    public class IncidentModel
    {
        private List<IncidentObject> _list = new List<IncidentObject>();
        public int ID { get; set; }
        public List<IncidentObject> IncidentModelList
        {
            get
            {
                return _list;
            }

            set
            {
                _list = value;
            }
        }
    }

    public class IncidentContext : DbContext
    {
        public DbSet<IncidentModel> IncidentModelContext { get; set; }
    }

}