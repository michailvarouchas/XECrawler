using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XECrawler
{
    public class XEphoneData
    {
        public bool flag { get; set; }
        public string phone { get; set; }
    } 

    public class XEphoneModel
    {
        public XEphoneData data { get; set; }
    }
}
