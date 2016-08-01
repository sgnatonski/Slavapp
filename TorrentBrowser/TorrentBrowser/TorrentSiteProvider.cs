using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentBrowser
{
    public static class TorrentSiteProvider
    {
        public static TorrentSite PirateBay => new TorrentSite
        {
            ListUrl = "https://thepiratebay.org/top/201",
            PageBaseUrl = "https://thepiratebay.org/",
            ListItemSelector = "#searchResult > tbody > tr > td > div > a"
        };

        public static TorrentSite ExtraTorrent => new TorrentSite
        {
            ListUrl = "http://extratorrent.cc/view/popular/Movies.html",
            PageBaseUrl = "http://extratorrent.cc",
            ListItemSelector = "table.tl > tbody > tr > td.tli > a"
        };

        public static TorrentSite _1337x => new TorrentSite
        {
            ListUrl = "http://1337x.to/top-100-movies",
            PageBaseUrl = "http://1337x.to",
            ListItemSelector = "div.featured-box.top-100 > div.tab-detail > ul > li > div.coll-1 > strong > a"
        };        
    }
}
