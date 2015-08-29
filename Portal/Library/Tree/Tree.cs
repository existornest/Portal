using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Portal.Library.Tree
{
    public class Tree
    {

        private List<Node> _consultantsList = new List<Node>();
        private List<Node> _coordinatorsList = new List<Node>();
        private List<Node> _salesDirectorsList = new List<Node>();
        private List<Node> _areaDirectorsList = new List<Node>();
        private string _html;

        public Tree()
        {
            
        }

        public List<Node> ConsultantsList
        {
            get
            {
                return _consultantsList;
            }

            set
            {
                _consultantsList = value;
            }
        }

        public List<Node> CoordinatorsList
        {
            get
            {
                return _coordinatorsList;
            }

            set
            {
                _coordinatorsList = value;
            }
        }

        public List<Node> SalesDirectorsList
        {
            get
            {
                return _salesDirectorsList;
            }

            set
            {
                _salesDirectorsList = value;
            }
        }

        public List<Node> AreaDirectorsList
        {
            get
            {
                return _areaDirectorsList;
            }

            set
            {
                _areaDirectorsList = value;
            }
        }

        public string Html
        {
            get
            {
                return _html;
            }
        }


        public string Render()
        {
            List<Node> freeSds = new List<Node>();
            List<Node> freeCds = new List<Node>();
            List<Node> freeCos = new List<Node>();

            _html = "<ul>";

            foreach (Node node in _areaDirectorsList.OrderBy(a => a.Name))
            {
                _html += "<li>" + node.Name + "/" + node.Guid + "/" + node.AdvFunction + "</li>";
                _html += "<li><ul>";

                foreach (Node sdNode in _salesDirectorsList.OrderBy(a => a.Name))
                {
                    if(sdNode.Parent == node.Guid)
                    {
                        _html += "<li>" + sdNode.Name + "/" + sdNode.Guid + "/" + sdNode.AdvFunction + "</li>";
                        _html += "<li><ul>";

                        foreach (Node cdNode in _coordinatorsList.OrderBy(a => a.Name))
                        {
                            if (cdNode.Parent == sdNode.Guid)
                            {
                                _html += "<li>" + cdNode.Name + "/" + cdNode.Guid + "/" + cdNode.AdvFunction + "</li>";

                                _html += "<li><ul>";

                                foreach (Node coNode in _consultantsList.OrderBy(a => a.Name))
                                {
                                    if (coNode.Parent == cdNode.Guid)
                                    {
                                        _html += "<li>" + coNode.Name + "/" + coNode.Guid + "/" + coNode.AdvFunction + "</li>";
                                    }
                                    else if(!freeCos.Contains(coNode))
                                    {
                                        freeCos.Add(coNode);
                                    }
                                }

                                _html += "</ul></li>";

                            }
                            else if(!freeCds.Contains(cdNode))
                            {
                                freeCds.Add(cdNode);
                            }
                        }

                        _html += "</ul></li>";

                    }
                    else if(!freeSds.Contains(sdNode))
                    {
                        freeSds.Add(sdNode);
                    }
                }
                _html += "</ul></li>";
            }

            foreach (Node sdNode in freeSds.OrderBy(a => a.Name))
            {
                _html += "<li>" + sdNode.Name + "/" + sdNode.Guid + "/" + sdNode.AdvFunction + "</li>";

                _html += "<ul>";
                foreach (Node cdNode in freeCds.OrderBy(a => a.Name))
                {
                    if (cdNode.Parent == sdNode.Guid)
                    {
                        _html += "<li>" + cdNode.Name + "/" + cdNode.Guid + "/" + cdNode.AdvFunction + "</li>";
                        _html += "<ul>";

                        foreach (Node coNode in freeCos.OrderBy(a => a.Name))
                        {
                            if (coNode.Parent == cdNode.Guid)
                            {
                                _html += "<li>" + coNode.Name + "/" + coNode.Guid + "/" + coNode.AdvFunction + "</li>";
                            }
                            
                        }

                        _html += "</ul>";
                    }
                }
                _html += "</ul>";

            }

            _html += "</ul>";

            return Html;
        }

        private void DeepSearch(Node node, List<Node> list)
        {
            _html += "<ul>";
            foreach (Node sub in list)
            {
                if (sub.Parent == node.Guid && sub.Parent != sub.Guid)
                {

                    _html += "<li><a href='/PortalNet/Cases?guid=" + sub.Guid + "&advFunction=" + sub.AdvFunction + "'>" + sub.Name + "</a></li>";
                    DeepSearch(sub, list);
                }
                
                
            }
            _html += "</ul>";
        }

        public string Render(string contactGuid)
        {
         

            List<Node> commonList = new List<Node>();
            commonList.AddRange(AreaDirectorsList);
            commonList.AddRange(SalesDirectorsList);
            commonList.AddRange(CoordinatorsList);
            commonList.AddRange(ConsultantsList);

            Node find = new Node();

            try
            {
                find = commonList.Where(a => a.ContactGuid == contactGuid).Select(row => row).First();
            }
            catch
            {
                
                _html = "";
                return Html;
            }

            _html += "<ul>";
            _html += "<li><a class='adv-own' href='/PortalNet/Cases?guid=" + find.Guid + "&advFunction=" + find.AdvFunction + "'>" + find.Name + "</a></li>";

            foreach (Node node in commonList.OrderBy(a => a.Name))
            {
                if(find.Guid == node.Parent)
                {
                    
                    _html += "<li><i class='fa fa-folder-open'></i>&nbsp;<a href='/PortalNet/Cases?guid=" + node.Guid + "&advFunction=" + node.AdvFunction + "'>" + node.Name + "</a></li>";
                    DeepSearch(node, commonList);
                }
            }

            _html += "</ul>";
            return Html;
        }


    }






    public class DuplicateNodeException: Exception
    {
        public DuplicateNodeException()
        {
            
        }

        public DuplicateNodeException(string m):base(m)
        {
            
        }
        public DuplicateNodeException(string m, Exception ex) : base(m, ex)
        {

        }
    }

}