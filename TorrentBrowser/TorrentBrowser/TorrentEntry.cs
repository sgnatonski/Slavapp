using System;

namespace TorrentBrowser
{
    public class TorrentEntry
    {
        public string Title { get; set; }
        public string Quality { get; set; }
        public string TorrentPage { get; set; }
        public Uri TorrentUri { get; set; }
    }
}
