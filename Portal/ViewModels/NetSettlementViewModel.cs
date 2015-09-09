using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.ViewModels
{

    public class NetSettlementObject
    {
        public string Source { get; set; }
        public Decimal Account { get; set; }
        public string Name { get; set; }
        public string DateOf { get; set; }
    }

    public class NetSettlementViewModel
    {

        private List<NetSettlementObject> _list = new List<NetSettlementObject>();

        public List<NetSettlementObject> NetSettlementList
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