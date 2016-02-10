using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
namespace testSite
{
    class cConfig
    {
        
        public cConfig() {
            using (XmlReader reader = XmlReader.Create(@"C:\CNP\cephinet_Config.xml"))
            {
                XmlDocument params_ = new XmlDocument();
                params_.Load(reader);

                XmlNode xmlnode = params_.DocumentElement.SelectSingleNode("/CEPHINET/Credentials");
                // if (xmlnode == null) return ...
                this.domain = xmlnode.Attributes["Domain"].Value;
                this.login = xmlnode.Attributes["Login"].Value;
                this.password = xmlnode.Attributes["Password"].Value;

                xmlnode = params_.DocumentElement.SelectSingleNode("/CEPHINET/Configuration/MasterRoot");
                // if (xmlnode == null) return ...
                this.rootDirectory = xmlnode.Attributes["directory"].Value;
                this.isRootDirectory = Convert.ToBoolean( xmlnode.Attributes["isRootDirectory"].Value );
                this.testMode = Convert.ToBoolean(xmlnode.Attributes["testMode"].Value);

                xmlnode = params_.DocumentElement.SelectSingleNode("/CEPHINET/Configuration/SpSiteRoot");
                // if (xmlnode == null) return ...
                this.spSiteRoot = xmlnode.Attributes["url"].Value;

                reader.Close();
            }
        }
          
        public string domain { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string rootDirectory { get; set; }
        public bool isRootDirectory { get; set; }
        public bool testMode { get; set; }
        public string spSiteRoot { get; set; }
    }
}
