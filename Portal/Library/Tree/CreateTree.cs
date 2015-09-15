using EarlyBoundTypes;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Portal.Library.Tree
{
    public class CreateTree
    {

        
        private const string AD = "Dyrektor Regionu";
        private const string SD = "Dyrektor Sprzedaży";
        private const string CD = "Koordynator";
        private const string CO = "Konsultant";

        private string _html = "";

        public string Html
        {
            get
            {
                return _html;
            }
        }

        

        public CreateTree(XrmServiceContext context, Guid userGuid)
        {


            string fetchxml = "<fetch>"+
                "<entity name='expl_obecnasiec' >" +
                "<attribute name='expl_dyrektorregionu' />"+
                "<attribute name='expl_dyrektor' />"+
                "<attribute name='expl_koordynator' />"+
                "<attribute name='expl_konsultant' />" +
                "<order attribute='expl_dyrektorregionu' descending='true' />" +
                "<order attribute='expl_dyrektor' descending='true' />" +
                "<order attribute='expl_koordynator' descending='true' />" +
                "<order attribute='expl_konsultant' descending='true' />" +
                "<filter type='or' >" +
                "<condition entityname='Konsultant' attribute='contactid' operator='eq' value='" + userGuid + "' />"+
                "<condition entityname='Koordynator' attribute='contactid' operator='eq' value='" + userGuid + "' /> "+
                "<condition entityname = 'Dyrektor' attribute ='contactid' operator= 'eq' value = '" + userGuid + "' />"+
                "<condition entityname='DyrektorRegionu' attribute='contactid' operator='eq' value='" + userGuid + "' />"+
                "</filter>"+
                "<link-entity name='expl_pracownik' from='expl_pracownikid' to='expl_konsultant' alias='a_konsultant' >"+
                "<attribute name='expl_name' />" +
                "<attribute name='expl_pracownikid' />" +
                "<link-entity name='contact' from='contactid' to='expl_kontakt' alias='Konsultant' >" +
                "<attribute name='contactid' alias='KonsultantID' />"+
                "</link-entity>"+
                "</link-entity>"+
                "<link-entity name = 'expl_koordynator' from = 'expl_koordynatorid' to = 'expl_koordynator' alias='a_koordynator' >"+
                "<attribute name='expl_name' />" +
                "<attribute name='expl_koordynatorid' />" +
                "<link-entity name = 'contact' from = 'contactid' to = 'expl_kontakt' alias = 'Koordynator' >" +
                "<attribute name='contactid' alias='KoordynatorID' />"+
                "</link-entity>"+
                "</link-entity>"+
                "<link-entity name='expl_dyrektorsprzedzay' from = 'expl_dyrektorsprzedzayid' to='expl_dyrektor' alias='a_dyrektor' >"+
                "<attribute name='expl_name' />" +
                "<attribute name='expl_dyrektorsprzedzayid' />" +
                "<link-entity name='contact' from='contactid' to='expl_kontakt' alias='Dyrektor' >" +
                "<attribute name='contactid' alias='DyrektorID' />"+
                "</link-entity>"+
                "</link-entity>"+
                "<link-entity name='expl_dyrektorregionu' from='expl_dyrektorregionuid' to='expl_dyrektorregionu' alias='a_dyrektorregionu' >" +
                "<attribute name='expl_name' />" +
                "<attribute name='expl_dyrektorregionuid' />" +
                "<link-entity name='contact' from='contactid' to='expl_kontakt' alias='DyrektorRegionu' >" +
                "<attribute name='contactid' alias='DyrektorRegionuID' />"+
                "</link-entity>"+
                "</link-entity>"+
                "</entity>"+
                "</fetch>";



            #region oldxml
            //string fetchxml =

            //@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >" +
            //"<entity name='expl_obecnasiec' >" +
            //"<attribute name='createdon' />" +
            //"<attribute name='expl_koordynator' />" +
            //"<attribute name='expl_konsultant' />" +
            //"<attribute name='expl_dyrektor' />" +
            //"<attribute name='expl_dyrektorregionu' />" +
            //"<attribute name='expl_obecnasiecid' />" +
            //"<order attribute='expl_dyrektorregionu' descending='true' />" +
            //"<order attribute='expl_dyrektor' descending='true' />" +
            //"<order attribute='expl_koordynator' descending='true' />" +
            //"<filter type='and' >" +
            //"<condition attribute='statecode' operator='eq' value='0' />" +
            //"</filter>" +
            //"<link-entity name='expl_pracownik' from='expl_pracownikid' to='expl_konsultant' visible='false' link-type='outer' alias='a_konsultant'>" +
            //"<attribute name='expl_name' />" +
            //"<attribute name='expl_pracownikid' />" +
            //"<attribute name='expl_kontakt' />" +
            //"</link-entity> " +
            //"<link-entity name='expl_koordynator' from='expl_koordynatorid' to='expl_koordynator' visible='false' link-type='outer' alias='a_koordynator' >" +
            //"<attribute name='expl_name' />" +
            //"<attribute name='expl_koordynatorid' />" +
            //"<attribute name='expl_kontakt' />" +
            //"</link-entity>" +
            //"<link-entity name='expl_dyrektorsprzedzay' from='expl_dyrektorsprzedzayid' to='expl_dyrektor' visible='false' link-type='outer' alias='a_dyrektor' >" +
            //"<attribute name='expl_name' />" +
            //"<attribute name='expl_dyrektorsprzedzayid' />" +
            //"<attribute name='expl_kontakt' />" +
            //"</link-entity>" +
            //"<link-entity name='expl_dyrektorregionu' from='expl_dyrektorregionuid' to='expl_dyrektorregionu' visible='false' link-type='outer' alias='a_dyrektorregionu' >" +
            //"<attribute name='expl_name' />" +
            //"<attribute name='expl_dyrektorregionuid' />" +
            //"<attribute name='expl_kontakt' />" +
            //"</link-entity>" +
            //"</entity ></fetch>";
            #endregion oldxml

            EntityCollection result = context.RetrieveMultiple(new FetchExpression(fetchxml));

            if(result.Entities.Count == 0)
            {
                _html = "";
                return;
            }

            string currentAreaDirector = null;
            string currentSalesDirector = null;
            string currentCoordinator = null;
            string currentConsultant = null;

            string currentAreaDirectorGuid = null;
            string currentSalesDirectorGuid = null;
            string currentCoordinatorGuid = null;
            string currentConsultantGuid = null;

            string currentAreaDirectorName = null;
            string currentAreaDirectorContactGuid = null;

            string currentSalesDirectorName = null;
            string currentSalesDirectorContactGuid = null;

            string currentCoordinatorName = null;
            string currentCoordinatorContactGuid = null;


            string currentConsultantName = null;
            string currentConsultantContactGuid = null;

            


            #region Query
            //IEnumerable<Entity> extracted = new List<Entity>();

            //extracted = (IEnumerable<Entity>)result.Entities

            //    .Where(a =>
            //    (((EntityReference)((AliasedValue)a.Attributes["a_dyrektorregionu.expl_kontakt"]).Value).Id != null && a.Contains("a_dyrektorregionu.expl_kontakt")) ? ((EntityReference)((AliasedValue)a.Attributes["a_dyrektorregionu.expl_kontakt"]).Value).Id == userGuid : false
            //    || (((EntityReference)((AliasedValue)a.Attributes["a_dyrektor.expl_kontakt"]).Value).Id != null && a.Contains("a_dyrektor.expl_kontakt")) ? ((EntityReference)((AliasedValue)a.Attributes["a_dyrektor.expl_kontakt"]).Value).Id == userGuid : false
            //    || (((EntityReference)((AliasedValue)a.Attributes["a_koordynator.expl_kontakt"]).Value).Id != null && a.Contains("a_koordynator.expl_kontakt")) ? ((EntityReference)((AliasedValue)a.Attributes["a_koordynator.expl_kontakt"]).Value).Id == userGuid : false
            //    || (((EntityReference)((AliasedValue)a.Attributes["a_konsultant.expl_kontakt"]).Value).Id != null && a.Contains("a_konsultant.expl_kontakt")) ? ((EntityReference)((AliasedValue)a.Attributes["a_konsultant.expl_kontakt"]).Value).Id == userGuid : false

            //    );
            #endregion Query

            _html += "<ul class='tree'>";

            bool firstLoop = true;
            
            
            foreach (Entity c in result.Entities)
            {

                if (c.Contains("a_dyrektorregionu.expl_name") && ((AliasedValue)c["a_dyrektorregionu.expl_name"]).Value != null)
                {
                    currentAreaDirectorName = (string)((AliasedValue)c["a_dyrektorregionu.expl_name"]).Value;
                    currentAreaDirectorGuid = ((AliasedValue)c["a_dyrektorregionu.expl_dyrektorregionuid"]).Value.ToString();
                    currentAreaDirectorContactGuid = ((AliasedValue)c["DyrektorRegionuID"]).Value.ToString();
                }
                else
                {
                    currentAreaDirectorName = null;
                    currentAreaDirectorGuid = null;
                    currentAreaDirectorContactGuid = null;
                }

                if (c.Contains("a_dyrektor.expl_name") && ((AliasedValue)c["a_dyrektor.expl_name"]).Value != null)
                {
                    currentSalesDirectorName = (string)((AliasedValue)c["a_dyrektor.expl_name"]).Value;
                    currentSalesDirectorGuid = ((AliasedValue)c["a_dyrektor.expl_dyrektorsprzedzayid"]).Value.ToString();
                    currentSalesDirectorContactGuid = ((AliasedValue)c["DyrektorID"]).Value.ToString();
                }
                else
                {
                    currentSalesDirectorName = null;
                    currentSalesDirectorGuid = null;
                    currentSalesDirectorContactGuid = null;
                }

                if (c.Contains("a_koordynator.expl_name") && ((AliasedValue)c["a_koordynator.expl_name"]).Value != null)
                {
                    currentCoordinatorName = (string)((AliasedValue)c["a_koordynator.expl_name"]).Value;
                    currentCoordinatorGuid = ((AliasedValue)c["a_koordynator.expl_koordynatorid"]).Value.ToString();
                    currentCoordinatorContactGuid = ((AliasedValue)c["KoordynatorID"]).Value.ToString();

                    

                }
                else
                {
                    currentCoordinatorName = null;
                    currentCoordinatorGuid = null;
                    currentCoordinatorContactGuid = null;
                    
                }



                if (c.Contains("a_konsultant.expl_name") && ((AliasedValue)c["a_konsultant.expl_name"]).Value != null)
                {
                    currentConsultantName = (string)((AliasedValue)c["a_konsultant.expl_name"]).Value;
                    currentConsultantGuid = ((AliasedValue)c["a_konsultant.expl_pracownikid"]).Value.ToString();
                    currentConsultantContactGuid = ((AliasedValue)c["KonsultantID"]).Value.ToString();

                }
                else
                {
                    currentConsultantName = null;
                    currentConsultantGuid = null;
                    currentConsultantContactGuid = null;
                }

                if (currentAreaDirector != currentAreaDirectorGuid)
                {
                    if(!firstLoop)
                    {
                        _html += "</ul></li></ul></li></ul></li>";
                        firstLoop = true;
                    }
                    

                    
                    currentAreaDirector = currentAreaDirectorGuid;

                    
                    if(currentAreaDirectorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html +=
                        "<li class='switch'><i class='fa fa-folder-open'></i><a class='AD' href='/PortalNet/Cases?guid=" + currentAreaDirectorGuid + "&advFunction=" + AD + "'>"
                        + ((currentAreaDirectorName == null) ? "NULL" : currentAreaDirectorName) + "</a><ul>";
                    }
                    else
                    {
                        _html +=
                       "<li><i class='fa fa-folder-open'></i><a class='AD' href='javascript:void(0);'>" + currentAreaDirectorName + "</a><ul>";
                    }
                    

                    currentSalesDirector = null;
                    currentCoordinator = null;

                }
   
                if (currentSalesDirector != currentSalesDirectorGuid)
                {

                    string additional = "";

                    if (!firstLoop)
                    {
                        _html += "</ul></li></ul></li>";
                        firstLoop = true;
                    }

                    if(null != currentAreaDirectorGuid)
                    {
                        additional = "&ad=" + currentAreaDirectorGuid;
                    }
                    

                    additional = "&ad=" + currentAreaDirectorGuid;

                    currentSalesDirector = currentSalesDirectorGuid;

                    if(currentSalesDirectorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html += "<li class='switch'><i class='fa fa-folder-open'></i><a class='SD' href='/PortalNet/Cases?guid=" + currentSalesDirectorGuid + "&advFunction=" + SD + 
                            additional + "'>" 
                        + ((currentSalesDirectorName == null) ? "NULL" : currentSalesDirectorName) + "</a><ul>";
                    }
                    else if(currentAreaDirectorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html += "<li class='switch'><i class='fa fa-folder-open'></i><a class='SD' href='/PortalNet/Cases?guid=" + currentSalesDirectorGuid + "&advFunction=" + SD + additional + "'>"
                       + ((currentSalesDirectorName == null) ? "NULL" : currentSalesDirectorName) + "</a><ul>";
                    }
                    else
                    {
                        _html +=
                        "<li><i class='fa fa-folder-open'></i><a class='SD' href='javascript:void(0);'>" + currentSalesDirectorName +  "</a><ul>";
                    }

                    

                    currentCoordinator = null;

                }


                if (currentCoordinator != currentCoordinatorGuid)
                {
                    string additional = "";

                    if (!firstLoop)
                    {
                        _html += "</ul></li>";
                        firstLoop = true;
                    }

                    if(null != currentSalesDirectorGuid && null != currentAreaDirectorGuid)
                    {
                        additional = "&ad=" + currentAreaDirectorGuid + "&sd=" + currentSalesDirectorGuid;
                    }
                    else if(null != currentSalesDirectorGuid && null == currentAreaDirectorGuid)
                    {
                        additional = "&sd=" + currentSalesDirectorGuid;
                    }

                    currentCoordinator = currentCoordinatorGuid;

                    if(currentCoordinatorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html +=
                        "<li class='switch'><i class='fa fa-folder-open'></i><a class='CD' href='/PortalNet/Cases?guid=" + currentCoordinatorGuid + "&advFunction=" + CD + additional + "'>"
                        + ((currentCoordinatorName == null) ? "NULL" : currentCoordinatorName) + "</a><ul>";
                    }
                    else if(currentSalesDirectorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html +=
                       "<li class='switch'><i class='fa fa-folder-open'></i><a class='CD' href='/PortalNet/Cases?guid=" + currentCoordinatorGuid + "&advFunction=" + CD + additional +"'>"
                       + ((currentCoordinatorName == null) ? "NULL" : currentCoordinatorName) + "</a><ul>";
                    }
                    else if (currentAreaDirectorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html +=
                     "<li><a class='CO' href='/PortalNet/Cases/?guid="
                     + currentCoordinatorGuid + "&advFunction=" + CD + "'>" + currentCoordinatorName + "</a></li>";
                    }

                    else
                    {
                        _html +=
                        "<li><i class='fa fa-folder-open'></i><a class='CD' href='javascript:void(0);'>" + currentCoordinatorName + "</a><ul>";
                    }

                    

                    currentConsultant = null;

                   

                }
                


                if (currentConsultant != currentConsultantGuid)
                {
                    string additional = "";

                    currentConsultant = currentConsultantGuid;

                    if(null != currentAreaDirectorGuid && null != currentSalesDirectorGuid && null != currentCoordinatorGuid)
                    {
                        additional = "&ad=" + currentAreaDirectorGuid + "&sd=" + currentSalesDirectorGuid + "&cd=" + currentCoordinatorGuid;
                    }
                    else if(null != currentSalesDirectorGuid && null != currentCoordinatorGuid && null == currentAreaDirectorGuid)
                    {
                        additional = "&sd=" + currentSalesDirectorGuid + "&cd=" + currentCoordinatorGuid;
                    }
                    else if(null != currentCoordinatorGuid && null == currentSalesDirectorGuid)
                    {
                        additional = "&cd=" + currentCoordinatorGuid;
                    }

                    
                    if(currentConsultantContactGuid.Equals(userGuid.ToString()))
                    {
                        _html +=
                       "<li><a class='CO' href='/PortalNet/Cases/?guid="
                       + currentConsultantGuid + "&advFunction=" + CO + additional + "'>" + currentConsultantName + "</a></li>";
                    }
                    else if(currentCoordinatorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html +=
                      "<li><a class='CO' href='/PortalNet/Cases/?guid="
                      + currentConsultantGuid + "&advFunction=" + CO + additional + "'>" + currentConsultantName + "</a></li>";
                    }
                    else if(currentSalesDirectorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html +=
                      "<li><a class='CO' href='/PortalNet/Cases/?guid="
                      + currentConsultantGuid + "&advFunction=" + CO + additional + "'>" + currentConsultantName + "</a></li>";
                    }
                    else if(currentAreaDirectorContactGuid.Equals(userGuid.ToString()))
                    {
                        _html +=
                     "<li><a class='CO' href='/PortalNet/Cases/?guid="
                     + currentConsultantGuid + "&advFunction=" + CO + additional + "'>" + currentConsultantName + "</a></li>";
                    }

                    else
                    {
                        _html +=
                        "<li><a class='CO' href='javascript:void(0);'>" + currentConsultantName + "</a></li>";
                    }

                    



                }


                firstLoop = false;

            }

            

            _html += "</ul></li></ul></li></ul></li></ul>";

        }

      

      




    }
}