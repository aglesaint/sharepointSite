using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace testSite
{
    class cLog
    {
        StreamWriter sw = null;
        public cLog(string filename)
        {
        //    int pos = filename.LastIndexOf(@"\");
        //    string sub = filename.Substring(Math.Max(0, filename.Length - pos));
            sw = new StreamWriter(filename);
            sw.WriteLine("\n" + DateTime.Now + " traitement \n");
        }

        public string adminSite { get; set; }

        public void WriteLog(string texte) {
           this.sw.WriteLine("\n" + texte);
        }

        public void CloseLog() {
            if (this.sw != null) this.sw.Close();
        }

    }
}
