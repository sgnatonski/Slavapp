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
            ListUrl = "http://extratorrent.cc/category/49/Thriller+Torrents.html",
            PageBaseUrl = "http://extratorrent.cc",
            ListItemSelector = "table.tl > tbody > tr > td.tli > a"
        };

        public static TorrentSite Kat => new TorrentSite
        {
            ListUrl = "https://kat.cr/movies/?field=seeders&sorder=desc",
            PageBaseUrl = "https://kat.cr/",
            ListItemSelector = "table > tbody > tr > td > div.torrentname > div > a"
        };
    }
}
