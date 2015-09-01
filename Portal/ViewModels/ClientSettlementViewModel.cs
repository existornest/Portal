using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.ViewModels
{
    public class ClientSettlementObject
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Case { get; set; }
        public Decimal Account { get; set; }
    }

    public class ClientSettlementViewModel
    {
        private List<ClientSettlementObject> _list = new List<ClientSettlementObject>();

        public string ClientName { get; set; }

        public List<ClientSettlementObject> ClientSettlementsList
        {
            get
            {
                return _list;
            }

            set
            {
                this._list = value;
            }

        }
    }

}