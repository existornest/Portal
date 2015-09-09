using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Library.Tree
{
    public class Node
    {
        private List<Node> _childNodes = new List<Node>();

        public List<Node> ChildNodes
        {
            get
            {
                return _childNodes;
            }

            set
            {
                _childNodes = value;
            }
        }

        public string ContactGuid { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public string AdvFunction { get; set; }
    }

   

    
}