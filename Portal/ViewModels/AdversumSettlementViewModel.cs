using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.ViewModels
{
    public class AdversumSettlementObject
    {
        public string Source { get; set; }
        public string Case { get; set; }
        public Decimal Account { get; set; }
        public string DateOf { get; set; }
        public string AdvancePayment { get; set; }
    }


    public class AdversumSettlementViewModel
    {
        private List<AdversumSettlementObject> _list = new List<AdversumSettlementObject>();

        public Decimal Summarize { get; set; }

        public List<AdversumSettlementObject> AdversumSettlmentsList
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