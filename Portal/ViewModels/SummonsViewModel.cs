using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.ViewModels
{
    public class SummonObject
    {
        public int ID { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public Decimal Account { get; set; }
        public string SendDate { get; set; }
        public string EndDate { get; set; }
    }

    public class SummonsViewModel
    {
        public string Client { get; set; }

        private List<SummonObject> _list = new List<SummonObject>();

        public List<SummonObject> list
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