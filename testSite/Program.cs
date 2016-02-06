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
          
           // on instancie le directory de travail
           DirectoryInfo rootDir = new DirectoryInfo(dirLoc);
           
           // fonction recursive pour parcourir l'arborescence
           WalkDirectoryTree(rootDir, myLog, "directoryParent");

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
                // on boucle sur les fichiers xml
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    if (dirParent.Equals("subDirectories"))
                        myLog.WriteLog("\n\tsous-site : " + root.Name);
                    else
                        myLog.WriteLog(fi.FullName);
                    
                    // on lit le fichier xml
                    using (XmlReader reader = XmlReader.Create(fi.FullName))
                    {
                           setXmlInfoFile(reader, fi.Name, myLog);
                           reader.Close();
                    }
                }
             
                // on boucle sur les subdirectories du directory parent.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {   
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo, myLog, "subDirectories");
                }
            }
        } //WalkDirectoryTree


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
                                if (reader.Read())
                                    myLog.WriteLog("\t\tsiteName: " + reader.Value.Trim());
                            }
                            break;
                    }
                }
            }  
        } // setXmlInfoFile
    } // class Program
}
