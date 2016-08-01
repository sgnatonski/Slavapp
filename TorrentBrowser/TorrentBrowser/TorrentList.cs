using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentBrowser
{
    public static class TorrentList
    {
        private static readonly string[] Filter = {"dvdscr", "camrip", "hdcam", ".tc.", "hdtc", "hdts", "hd-ts"};

        public static async Task<IEnumerable<TorrentEntry>> GetTorrents(TorrentSite site, CancellationToken cancellationToken)
        {
            var document = await PirateRequest.OpenAsync(new Uri(site.ListUrl), cancellationToken);
            var entries = document.QuerySelectorAll(site.ListItemSelector)
                .Select(c => new { Text = c.TextContent, Href = c.GetAttribute("href")?.Trim() })
                .Select(m => new TorrentEntry
                {
                    Title = m.Text,
                    Quality = TorrentQualityExtractor.ExtractQuality(m.Text),
                    TorrentPage = site.PageBaseUrl + m.Href,
                    TorrentUri = new Uri(site.PageBaseUrl + m.Href)
                });

            return FilterMovies(entries);
        }

        private static IEnumerable<TorrentEntry> FilterMovies(IEnumerable<TorrentEntry> movies)
        {
            return movies
                .Select(x => new { Torrent = x, TorrentPage = (x.TorrentPage ?? string.Empty).ToLower() })
                .Where(p => !Filter.Any(p.TorrentPage.Contains))
                .Select(x => x.Torrent);
        }
    }
}
