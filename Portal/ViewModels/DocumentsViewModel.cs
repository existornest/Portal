using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.ViewModels
{
    public class DocumentsViewObject
    {
        public string CaseID { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string DateOf { get; set; }
    }

    public class DocumentViewModel
    {
       private List<DocumentsViewObject> _list = new List<DocumentsViewObject>();

        public List<DocumentsViewObject> DocsList
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