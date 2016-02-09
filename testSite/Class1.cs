using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SharePoint.Client;
using System.Net;
using System.Xml;
using System.IO;

namespace testSite
{
    class CSOMCalls
    {

        /// <summary>
        /// Verification de l'existance d'un site
        /// </summary>
        /// <param name="url_"></param>
        /// <returns>vrai si trouvé, false si non trouvé</returns>
        public bool CheckSite(string url_, XmlDocument params_)
        {
            try
            {
                using (ClientContext ctx = new ClientContext(url_))
                {
                    ctx.Credentials = SelectCreds(params_);
                    Web webSite = ctx.Web;
                    ctx.Load(webSite);
                    ctx.ExecuteQuery();

                    return true;
                }
            }
            catch (WebException webex)
            {
                HttpWebResponse errorResponse = webex.Response as HttpWebResponse;
                if (errorResponse == null) throw webex;
                if (errorResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw webex;
            }
            catch (ClientRequestException)
            {
                return false;
            }
        }


        /// <summary>
        /// Suppression d'un site
        /// </summary>
        /// <param name="url_"></param>
        /// <returns></returns>
        public void DeleteSite(string urlRoot_, string urlSite_, XmlDocument params_)
        {
            using (ClientContext ctx = new ClientContext(urlRoot_))
            {
                ctx.Credentials = SelectCreds(params_);
                Web w = ctx.Web;
                GroupCollection gc = w.SiteGroups;
                ctx.Load(w);
                ctx.Load(gc);
                //ctx.ExecuteQuery();


                // Supression des groupes 
                XmlNode groups = params_.DocumentElement.SelectSingleNode("/CNPCloud/GroupsToCreate");
                foreach (XmlNode xng in groups.ChildNodes)
                {
                    try
                    {
                        gc.Remove(gc.GetByName(urlSite_ + xng.Attributes["Suffix"].Value));
                        ctx.ExecuteQuery();

                    }
                    catch (Exception) { } // On ne fait rien, les groupes n'existent peut etre pas.
                }

                // Suppression du groupe admin
                XmlNode groupAdmin = params_.DocumentElement.SelectSingleNode("/CNPCloud/GroupAdmin");
                try
                {
                    gc.Remove(gc.GetByName(urlSite_ + groupAdmin.Attributes["Suffix"].Value));
                    ctx.ExecuteQuery();
                }
                catch (Exception) { } // On ne fait rien, les groupes n'existent peut etre pas.
            }


            string webUrl = urlRoot_ + (urlRoot_.Substring(urlRoot_.Length - 1).Equals('/') ? "" : "/") + urlSite_;
            using (ClientContext ctx = new ClientContext(webUrl))
            {
                ctx.Credentials = SelectCreds(params_);
                ctx.Web.DeleteObject();
                ctx.ExecuteQuery();
            }
        }


        public string CreateSite(string urlRoot_, string siteUrl_, string title_, string administrator_, XmlDocument params_)
        {

            using (ClientContext ctx = new ClientContext(urlRoot_))
            {
                ctx.Credentials = SelectCreds(params_);
                Web rootWeb = ctx.Web;
                ctx.Load(rootWeb);
                ctx.ExecuteQuery();

                // Site web
                WebCreationInformation wci = new WebCreationInformation();
                wci.Url = siteUrl_;
                wci.Title = title_;
                wci.Language = Convert.ToInt32(params_.DocumentElement.Attributes["Langue"].Value);
                wci.WebTemplate = params_.DocumentElement.Attributes["Template"].Value;
                wci.Description = "";
                wci.UseSamePermissionsAsParentSite = false;
                Web newWeb = ctx.Web.Webs.Add(wci);
               

                // Paramétrage du site
                // Masterpage
                /*         newWeb.MasterUrl = ctx.Web.ServerRelativeUrl + params_.DocumentElement.Attributes["MasterPage"].Value;
                         newWeb.CustomMasterUrl = ctx.Web.ServerRelativeUrl + params_.DocumentElement.Attributes["MasterPage"].Value;
                         // Features à desactiver
                         XmlNode feats = params_.DocumentElement.SelectSingleNode("/CNPCloud/FeaturesToDeactivate");
                         foreach (XmlNode xnf in feats.ChildNodes)
                         {
                             newWeb.Features.Remove(new Guid(xnf.Attributes["Id"].Value), true);
                         }
                 */

                /* 
                 * Groupe administrateur du site en cours 
                 * 
                 */
                  XmlNode groupAdmin = params_.DocumentElement.SelectSingleNode("/CNPCloud/GroupAdmin");
                  GroupCreationInformation gcadmin = new GroupCreationInformation();
                  gcadmin.Title = siteUrl_ + groupAdmin.Attributes["Suffix"].Value;
                  Group gAdmins = newWeb.SiteGroups.Add(gcadmin);
                  gAdmins.Owner = ctx.Web.EnsureUser(params_.DocumentElement.Attributes["SPAdmin"].Value);
                  UserCreationInformation uci = new UserCreationInformation();
                  uci.LoginName = administrator_;
                  gAdmins.Users.Add(uci);
                  gAdmins.Update();
    
                  SetRoleForGroup(ctx, newWeb, gAdmins, groupAdmin.Attributes["Role"].Value);
                  newWeb.AssociatedOwnerGroup = gAdmins;
                  newWeb.Update();

                  /* 
                   * Creation des groupes supplémentaire 
                   * ex: <GroupsToCreate>	
                   *       <Group Suffix="_Collaborateurs" Role="Modification"/>
                   */
                  XmlNode groups = params_.DocumentElement.SelectSingleNode("/CNPCloud/GroupsToCreate");
                     foreach (XmlNode xng in groups.ChildNodes)
                     {
                         GroupCreationInformation gci = new GroupCreationInformation();
                         gci.Title = siteUrl_ + xng.Attributes["Suffix"].Value;
                         Group g = newWeb.SiteGroups.Add(gci);
                         g.Owner = gAdmins;
                         g.Update();
                         SetRoleForGroup(ctx, newWeb, g, xng.Attributes["Role"].Value);
                     }





                     /*
                     GroupCreationInformation gcAdmin = new GroupCreationInformation();
                     gcAdmin.Title = siteUrl_ + "_Admins";
                    gAdmins = newWeb.SiteGroups.Add(gcAdmin);
                     gAdmins.Owner = ctx.Web.EnsureUser(params_.Attributes["SPAdmin"].Value);
                     UserCreationInformation uci = new UserCreationInformation();
                     uci.LoginName = administrator_;
                     gAdmins.Users.Add(uci);
                     gAdmins.Update();
                     SetRoleForGroup(ctx, newWeb, gAdmins, RoleType.WebDesigner);
                     /*
                      // Collab
                      GroupCreationInformation gcCollab = new GroupCreationInformation();
                      gcCollab.Title = siteUrl_ + "_Collaborateurs";
                      Group gCollab = newWeb.SiteGroups.Add(gcCollab);
                      gCollab.Owner = gAdmins;
                      gCollab.Update();
                      SetRoleForGroup(ctx, newWeb, gCollab, RoleType.Contributor);

                      // Lecteur
                      GroupCreationInformation gcVisit = new GroupCreationInformation();
                      gcVisit.Title = siteUrl_ + "_Visiteurs";
                      Group gVisit = newWeb.SiteGroups.Add(gcVisit);
                      gVisit.Owner = gAdmins;
                      gVisit.Update();
                      SetRoleForGroup(ctx, newWeb, gVisit, RoleType.Reader);
                      */


                ctx.ExecuteQuery();

                return "OK";
            }

        }



        private void SetRoleForGroup(ClientContext clientContext, Web oWebsite, Group group, string role)
        {
            RoleDefinitionBindingCollection collRoleDefinitionBinding = new RoleDefinitionBindingCollection(clientContext);
            RoleDefinition oRoleDefinition = oWebsite.RoleDefinitions.GetByName(role);
            collRoleDefinitionBinding.Add(oRoleDefinition);
            oWebsite.RoleAssignments.Add(group, collRoleDefinitionBinding);

        }


        private ICredentials SelectCreds(XmlDocument params_)
        {

            // test cred
            return new NetworkCredential(@"CEPHINET\Administrateur", "spiu90WjV");
            

            /*
            XmlNode xmlCreds = params_.DocumentElement.SelectSingleNode("/CNPCloud/Credentials");
            if (xmlCreds == null) return CredentialCache.DefaultCredentials;
            else return new NetworkCredential(xmlCreds.Attributes["Login"].Value, xmlCreds.Attributes["Password"].Value, xmlCreds.Attributes["Domain"].Value);
              */
        }
    }
}
