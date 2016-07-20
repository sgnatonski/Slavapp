using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentBrowser
{
    public class TorrentSite
    {
        public string ListUrl { get; set; }
        public string PageBaseUrl { get; set; }
        public string ListItemSelector { get; set; }
    }
}
