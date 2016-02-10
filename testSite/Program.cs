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
           cConfig myConfig = new cConfig();  
           cLog myLog = new cLog(myConfig.rootDirectory + ".txt");

           // activer le mode test, on ne passe pas par le CSOM
           myLog.WriteLog("\nmode test " + myConfig.testMode);

           // administrateur de site
           myLog.adminSite = @"CEPHINET\Administrateur";
           myLog.WriteLog("\nadministrator_ (ensemble des sites) : " + myLog.adminSite + "\n");

           // on instancie le directory de travail
           DirectoryInfo rootDir = new DirectoryInfo(myConfig.rootDirectory);
         
           // site root
           myConfig.spSiteRoot = myConfig.spSiteRoot + rootDir.Name;
           string currentUrl = myConfig.spSiteRoot;
           // fonction recursive pour parcourir l'arborescence
           WalkDirectoryTree(rootDir, currentUrl, myLog, myConfig);
           myLog.CloseLog();
        } // class main
        
        // parcours de l'arborescence
        static void WalkDirectoryTree(System.IO.DirectoryInfo root, string currentUrl, cLog myLog, cConfig myConfig)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            if (!myConfig.isRootDirectory) currentUrl = myConfig.spSiteRoot + "/" + root.Name;
           
            myLog.WriteLog("\t(currentUrl) : " + currentUrl); 
            
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
                    // on lit le fichier xml en cours
                    using (XmlReader reader = XmlReader.Create(fi.FullName))
                    {
                        setXmlInfoFile(reader, fi.Name, myLog);
                       // on ne traite pas le xml contant le nom, ni le root de 1er niveau
                        if (!fi.Name.Equals("folderName.xml"))
                           callCSOM(reader, currentUrl, myLog, myConfig);

                        reader.Close();
                    }
                } // foreach
              } //if

                /* 
                 * on boucle de maniere recursives
                 * sur les subdirectories du directory actuel.
                 */
                myConfig.isRootDirectory = false;
                subDirs = root.GetDirectories();
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {   
                    // appel recursif pour chaque sub directory
                    WalkDirectoryTree(dirInfo, currentUrl, myLog, myConfig);
                }
        } //WalkDirectoryTree

        /* 
        * appel du CSOM avec le second fichier XML
        */
        static bool callCSOM(XmlReader reader, string currentUrl, cLog myLog, cConfig myConfig)
        {
            // on traite le CSOM une fois les 2 fichiers xml parses
            if (reader != null)
            {
                XmlDocument params_ = new XmlDocument();
                params_.Load(reader);

                string siteUrl_ = myLog.siteUrl;
                string title_ = myLog.title;
                string administrator_ = myLog.adminSite;

                myLog.WriteLog(Environment.NewLine + "========== on passe par le CSOM ==========");

                    try
                    {
                        CSOMCalls objSite = new CSOMCalls();
                        /* 
                         * appel du CSOM avec le second fichier XML
                         * on contrôle l'existence du site
                         * ex :  siteUrl_ = "http://sharepoint.cephinet.info/my/personal/administrateur/testAuto9/";
                         */
                        if (!objSite.CheckSite(currentUrl, params_)) { 
                            if (!myConfig.testMode)
                            {
                                objSite.CreateSite(currentUrl, siteUrl_, title_, administrator_, params_);
                            }        
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
                        case "name":
                            if (xmlfileName.Equals("folderName.xml"))
                            {
                                if (reader.Read()) {
                                    myLog.siteUrl = reader.Value.Trim();
                                    myLog.WriteLog("\t(old) siteUrl_: " + reader.Value.Trim()); 
                                }
                            }
                            break;
                        case "h_Name":
                            if (reader.Read())  {
                                myLog.title = reader.Value.Trim();
                                myLog.WriteLog("\t(node : h_Name) title_: " + reader.Value.Trim());
                             }
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