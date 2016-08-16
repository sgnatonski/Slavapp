using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentBrowser
{
    public class TorrentImdbEntry
    {
        public int Id { get; set; }
        public Uri TorrentLink { get; set; }
        public int ImdbId { get; set; }
        public Uri ImdbLink { get; set; }

        public bool IsValid => ImdbLink != null && ImdbId > 0;
    }
}
