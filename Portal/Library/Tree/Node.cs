using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Library.Tree
{
    public struct Node
    {
        public string ContactGuid { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public string AdvFunction { get; set; }
    }
}