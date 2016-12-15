using System;
using System.Linq;

namespace TorrentBrowser
{
    public class TorrentMovie
    {
        public int Id { get; set; }
        public Uri TorrentLink { get; set; }
        public Uri ImdbLink { get; set; }
        public Uri PictureUrl { get; set; }
        public string Movie { get; set; }
        public float Rating { get; set; }
        public string Quality { get; set; }
        public SubtitleData[] Subtitles { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool HasImdbLink { get { return ImdbLink != null; } }
        public bool HasTorrentLink { get { return TorrentLink != null; } }
        public bool HasSubtitles { get { return Subtitles != null && Subtitles.Any(); } }
        public bool SubtitlesMatch
        {
            get
            {
                return HasSubtitles && Subtitles.Any(s => !string.IsNullOrEmpty(s.ReleaseName) && TorrentLink.ToString().ToLower().Contains(s.ReleaseName.ToLower()));
            }
        }
    }
}
