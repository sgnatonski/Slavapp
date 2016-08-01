using System;

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
