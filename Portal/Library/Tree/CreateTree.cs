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

        private Tree _tree = new Tree();
        private const string AD = "Dyrektor Regionu";
        private const string SD = "Dyrektor Sprzedaży";
        private const string CD = "Koordynator";
        private const string CO = "Konsultant";

        public CreateTree(XrmServiceContext context)
        {
            

            string fetchxml =

            @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >" +
            "<entity name='expl_obecnasiec' >" +
            "<attribute name='createdon' />" +
            "<attribute name='expl_koordynator' />" +
            "<attribute name='expl_konsultant' />" +
            "<attribute name='expl_dyrektor' />" +
            "<attribute name='expl_dyrektorregionu' />" +
            "<attribute name='expl_obecnasiecid' />" +
            "<order attribute='expl_dyrektorregionu' descending='true' />" +
            "<order attribute='expl_dyrektor' descending='true' />" +
            "<order attribute='expl_koordynator' descending='true' />" +
            "<filter type='and' >" +
            "<condition attribute='statecode' operator='eq' value='0' />" +
            "</filter>" +
            "<link-entity name='expl_pracownik' from='expl_pracownikid' to='expl_konsultant' visible='false' link-type='outer' alias='a_konsultant'>" +
            "<attribute name='expl_name' />" +
            "<attribute name='expl_pracownikid' />" +
            "</link-entity> " +
            "<link-entity name='expl_koordynator' from='expl_koordynatorid' to='expl_koordynator' visible='false' link-type='outer' alias='a_koordynator' >" +
            "<attribute name='expl_name' />" +
            "<attribute name='expl_koordynatorid' />" +
            "</link-entity>" +
            "<link-entity name='expl_dyrektorsprzedzay' from='expl_dyrektorsprzedzayid' to='expl_dyrektor' visible='false' link-type='outer' alias='a_dyrektor' >" +
            "<attribute name='expl_name' />" +
            "<attribute name='expl_dyrektorsprzedzayid' />" +
            "</link-entity>" +
            "<link-entity name='expl_dyrektorregionu' from='expl_dyrektorregionuid' to='expl_dyrektorregionu' visible='false' link-type='outer' alias='a_dyrektorregionu' >" +
            "<attribute name='expl_name' />" +
            "<attribute name='expl_dyrektorregionuid' />" +
            "<attribute name='expl_kontakt' />" +
            "</link-entity>" +
            "</entity ></fetch>";


            EntityCollection result = context.RetrieveMultiple(new FetchExpression(fetchxml));

            Node areaDirector = new Node();
            Node salesDirector = new Node();
            Node coordinator = new Node();
            Node consultant = new Node();

            bool areaDirectorNull = true;
            bool salesDirectorNull = true;
            bool coordinatorNull = true;
            bool consultantNull = true;

            foreach (Entity c in result.Entities)
            {
                
                if (c.Contains("a_dyrektorregionu.expl_name") && ((AliasedValue)c["a_dyrektorregionu.expl_name"]).Value != null)
                {
                    string name = (string)((AliasedValue)c["a_dyrektorregionu.expl_name"]).Value;
                    string guid = ((AliasedValue)c["a_dyrektorregionu.expl_dyrektorregionuid"]).Value.ToString();

                    try
                    {
                        areaDirector = _tree.AreaDirectorsList.Where(a => a.Guid == guid).Select(row => row).First();
                    }
                    catch
                    {

                        Guid contactGuid = context.expl_dyrektorregionuSet
                            .Where(a => a.Id == new Guid(guid)).Select(b => b.expl_kontakt.Id).FirstOrDefault();

                        areaDirector = new Node()
                        {
                            ContactGuid = contactGuid.ToString(),
                            Guid = guid,
                            Parent = guid,
                            Name = name,
                            AdvFunction = AD
                        };

                        _tree.AreaDirectorsList.Add(areaDirector);
                    }

                    areaDirectorNull = false;

                }
                else
                {
                    areaDirectorNull = true;
                }


                if (c.Contains("a_dyrektor.expl_name") && ((AliasedValue)c["a_dyrektor.expl_name"]).Value != null)
                {
                    string name = (string)((AliasedValue)c["a_dyrektor.expl_name"]).Value;
                    string guid = ((AliasedValue)c["a_dyrektor.expl_dyrektorsprzedzayid"]).Value.ToString();

                    try
                    {
                        salesDirector = _tree.SalesDirectorsList.Where(a => a.Guid == guid).Select(row => row).First();
                    }
                    catch
                    {

                        Guid contactGuid = context.expl_dyrektorsprzedzaySet
                            .Where(a => a.Id == new Guid(guid)).Select(b => b.expl_Kontakt.Id).FirstOrDefault();

                        salesDirector = new Node()
                        {
                            ContactGuid = contactGuid.ToString(),
                            Guid = guid,
                            Parent = areaDirectorNull ? guid : areaDirector.Guid,
                            Name = name,
                            AdvFunction = SD
                        };

                        _tree.SalesDirectorsList.Add(salesDirector);
                    }

                    salesDirectorNull = false;
                }
                else
                {
                    salesDirectorNull = true;
                }


                if (c.Contains("a_koordynator.expl_name") && ((AliasedValue)c["a_koordynator.expl_name"]).Value != null)
                {
                    string name = (string)((AliasedValue)c["a_koordynator.expl_name"]).Value;
                    string guid = ((AliasedValue)c["a_koordynator.expl_koordynatorid"]).Value.ToString();

                    try
                    {
                        coordinator = _tree.CoordinatorsList.Where(a => a.Guid == guid).Select(row => row).First();
                    }
                    catch
                    {
                        Guid contactGuid = context.expl_koordynatorSet
                            .Where(a => a.Id == new Guid(guid)).Select(b => b.expl_Kontakt.Id).FirstOrDefault();

                        coordinator = new Node()
                        {
                            ContactGuid = contactGuid.ToString(),
                            Guid = guid,
                            Parent = salesDirectorNull ? guid : salesDirector.Guid,
                            Name = name,
                            AdvFunction = CD
                        };

                        _tree.CoordinatorsList.Add(coordinator);
                    }

                    coordinatorNull = false;
                }
                else
                {
                    coordinatorNull = true;
                }


                if (c.Contains("a_konsultant.expl_name") && ((AliasedValue)c["a_konsultant.expl_name"]).Value != null)
                {
                    string name = (string)((AliasedValue)c["a_konsultant.expl_name"]).Value;
                    string guid = ((AliasedValue)c["a_konsultant.expl_pracownikid"]).Value.ToString();

                    try
                    {
                        consultant = _tree.ConsultantsList.Where(a => a.Guid == guid).Select(row => row).First();
                    }
                    catch
                    {

                        Guid contactGuid = context.expl_pracownikSet
                            .Where(a => a.Id == new Guid(guid)).Select(b => b.expl_Kontakt.Id).FirstOrDefault();

                        consultant = new Node()
                        {
                            ContactGuid = contactGuid.ToString(),
                            Guid = guid,
                            Parent = coordinatorNull ? guid : coordinator.Guid,
                            Name = name,
                            AdvFunction = CO
                        };

                        _tree.ConsultantsList.Add(consultant);
                    }

                    consultantNull = false;

                }
                else
                {
                    consultantNull = true;
                }


                


            }



        }



        public string Render()
        {
            return _tree.Render();
        }

        public string Render(string guid)
        {
            return _tree.Render(guid);
        }


    }
}