using System.Linq;

namespace TorrentBrowser
{
    public static class TorrentQualityExtractor
    {
        public static string ExtractQuality(string moviename)
        {
            var m = moviename.ToLower().Split(' ', '.', '_', '(', ')', '[', ']');
            var tags = new[]
            {
                "dvdscr", "camrip", "hdcam", "tc", "hdtc", "hdts", "hd-ts",
                "hdrip", "hd-rip", "dvdrip", "brrip", "bdrip", "webrip"
            };

            return string.Join(" ", m.Intersect(tags)) ?? string.Empty;
        }
    }
}
