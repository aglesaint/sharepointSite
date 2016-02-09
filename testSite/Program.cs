using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Microsoft.SharePoint.Client;
using System.IO;
using System.Xml;
namespace testSite
{
    class Program
    {

        static void Main(string[] args)
        {
           // directory root de travail
           string dirLoc = @"c:\CNP\Extranet Cephinet - Centre Karate Nimois";
           // on instancie la classe log
           cLog myLog = new cLog(dirLoc+".txt"); // TODO
           // sharepoint root site
           myLog.spRoot = @"http://sharepoint.cephinet.info/my/personal/administrateur/"; 
           // activer le mode test, on ne passe pas par le CSOM
           myLog.testMode = true;
            
           // administrateur de site
           myLog.adminSite = @"CEPHINET\Administrateur";
           myLog.WriteLog("\nadministrator_ (ensemble des sites) : " + myLog.adminSite + "\n");
        
           // on instancie le directory de travail
           DirectoryInfo rootDir = new DirectoryInfo(dirLoc);

           // fonction recursive pour parcourir l'arborescence
           WalkDirectoryTree(rootDir, myLog, "directoryParent");
           myLog.CloseLog();
                   /* 
                       try
                       {
                           // 
                           // info de base, pour la réalisation de tests manuel

                         string urlRoot_ = "http://sharepoint.cephinet.info/my/personal/administrateur";
                         string siteUrl_ = "testAuto10";
                         string title_ = "autoCreate10";
                         string administrator_ = @"CEPHINET\Administrateur";
             
                         // fichier xml
                         XmlDocument params_ = new XmlDocument();
                         params_.Load(reader);
                         // log
                        // myLog.WriteLog(params_.OuterXml);

                         CSOMCalls objSite = new CSOMCalls();
                         objSite.CreateSite(urlRoot_, siteUrl_, title_, administrator_, params_);
                       }
                       catch (Exception ex)
                       {
                          // myLog.WriteLog("\n" + ex);
                       }
                       finally
                       {
                          // myLog.CloseLog();
                           if (reader != null) reader.Close();
                       }
               */

        } // class main
        
        // parcours de l'arborescence
        static void WalkDirectoryTree(System.IO.DirectoryInfo root, cLog myLog, string dirParent)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // recuperer les fichiers xml
            try
            {
                files = root.GetFiles("*.xml");
            }
            // on leve une exception si un fichier requiert une autorisation
            catch (UnauthorizedAccessException e)
            {
                myLog.WriteLog(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                myLog.WriteLog(e.Message);
            }
            
            if (files != null)
            {
                // on boucle sur 2 les fichiers xml de chaque directory
                foreach (System.IO.FileInfo fi in files)
                {
                    if (dirParent.Equals("subDirectories"))
                    {
                        myLog.WriteLog(Environment.NewLine);
                        // positionner le site parent
                        // TODO
                        // urlRoot
                        myLog.WriteLog("\n\tsous-site : " + root.Name);
                    }
                    else {
                        myLog.urlRoot = myLog.spRoot + fi.FullName;
                        myLog.WriteLog("\nurlRoot_ : " + fi.FullName);
                    }
                    // on lit le fichier xml en cours
                    using (XmlReader reader = XmlReader.Create(fi.FullName))
                    {
                        setXmlInfoFile(reader, fi.Name, myLog);
                       // on traite le xml contant l'arborescence SP
                       if (!fi.Name.Equals("folderName.xml"))
                            callCSOM(reader, myLog);

                        reader.Close();
                    }
                } // foreach
                
              } //if

                /* 
                 * on boucle de maniere recursives
                 * sur les subdirectories du directory actuel.
                 *  
                 * 
                 */
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {   
                    // appel recursif pour chaque sub directory
                    WalkDirectoryTree(dirInfo, myLog, "subDirectories");
                }
        } //WalkDirectoryTree

        /* 
        * appel du CSOM avec le second fichier XML
        */
        static bool callCSOM(XmlReader reader, cLog myLog)
        {
            // on traite le CSOM une fois les 2 fichiers xml parses
            if (reader != null)
            {
                XmlDocument params_ = new XmlDocument();
                params_.Load(reader);

                string urlRoot_ = myLog.urlRoot;
                string siteUrl_ = myLog.siteUrl;
                string title_ = myLog.title;
                string administrator_ = myLog.adminSite;

                myLog.WriteLog("========== on passe par le CSOM ==========");

                    try
                    {
                        CSOMCalls objSite = new CSOMCalls();
                        /* 
                         * appel du CSOM avec le second fichier XML
                         * on contrôle l'existence du site
                         * ex :  siteUrl_ = "http://sharepoint.cephinet.info/my/personal/administrateur/testAuto9/";
                         */
                        if (!objSite.CheckSite(siteUrl_, params_)){ 
                            if (!myLog.testMode)
                            {
                                objSite.CreateSite(urlRoot_, siteUrl_, title_, administrator_, params_);
                            }
                            else
                                myLog.WriteLog("\nmode test activé : le site est crée ici " + urlRoot_);
                        }
                        else
                            myLog.WriteLog("\n le site : " + siteUrl_ + " existe déjà");
                    }
                    catch (Exception ex)
                    {
                        myLog.WriteLog("\n" + ex);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
            }
            return true; // TODO
        }

        /* 
         * lecture des fichiers xml
         * 
         */
        static void setXmlInfoFile(XmlReader reader, string xmlfileName, cLog myLog){
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    // Get element name and switch on it.
                    switch (reader.Name)
                    {
                        case "folder":
                            break;
                        case "name":
                            if (xmlfileName.Equals("folderName.xml"))
                            {
                                if (reader.Read()) {
                                    myLog.siteUrl = reader.Value.Trim();
                                    myLog.WriteLog("\tsiteUrl_: " + reader.Value.Trim());
                                }
                            }
                            break;
                        case "title":
                            if (reader.Read())  {
                                myLog.title = reader.Value.Trim();
                                myLog.WriteLog("\ttitle_: " + reader.Value.Trim());
                             }
                            break;
                        case "formName":
                            if (reader.Read())
                                myLog.WriteLog("\ttitle_: " + reader.Value.Trim());
                            break;
                        case "h_Authors":
                            if (reader.Read())
                                myLog.WriteLog("\tauthors_: " + reader.Value.Trim());
                            break;
                        case "attachmentName":
                             if (reader.Read())
                                 myLog.WriteLog("\tattach_: " + reader.Value.Trim());
                            break;
                    }
                }
            }  
        } // setXmlInfoFile
    } // class Program
}
