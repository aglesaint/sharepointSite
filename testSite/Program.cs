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
            StreamWriter sw = new StreamWriter("c:/test_loc.txt");
            string texte = DateTime.Now + " traitement \n";
            sw.WriteLine("\n" + texte);
           
            String filename = @"c:\fr.cnp.sharepoint.CNPCloud.xml";
            XmlReader reader = XmlReader.Create(filename);
            try
            {
                /* 
                 * info de base, pour la réalisation de tests manuel
                 * 
                 */
              string urlRoot_ = "http://sharepoint.cephinet.info/my/personal/administrateur";
              string siteUrl_ = "testAuto10";
              string title_ = "autoCreate10";
              string administrator_ = @"CEPHINET\Administrateur";
             
              // fichier xml
              XmlDocument params_ = new XmlDocument();
              params_.Load(reader);
              // log
              sw.WriteLine(params_.OuterXml);

              CSOMCalls objSite = new CSOMCalls();
              objSite.CreateSite(urlRoot_, siteUrl_, title_, administrator_, params_);
            }
            catch (Exception ex)
            {
                sw.WriteLine("\n" + ex);
            }
            finally
            {
                if(sw != null) sw.Close();
                if (reader != null) reader.Close();
            }
        }
    }
}
