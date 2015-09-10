using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.ViewModels
{

    public class PoliciesObject
    {
        public int ID { get; set; }
        public string InsuranceNumber { get; set; }
        public string  Company{ get; set; }
        public string InsuranceType { get; set; }
        public string Contact { get; set; }
        public string Account { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }

    }

    public class PoliciesViewModel
    {

        private List<PoliciesObject> _list = new List<PoliciesObject>();

        public List<PoliciesObject> PoliciesList
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
}