using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentBrowser
{
    public class ImdbData
    {
        public int Id { get; set; }
        public string MovieName { get; set; }
        public float? Rating { get; set; }
        public Uri PictureLink { get; set; }

    }
}
