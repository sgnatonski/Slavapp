using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace TorrentBrowser
{
    public static class TorrentList
    {
        private static readonly string[] Filter = {"dvdscr", "camrip", "hdcam", ".tc.", "hdtc", "hdts", "hd-ts"};

        public static IEnumerable<TorrentEntry> GetTorrents(TorrentSite site, CancellationToken cancellationToken)
        {
            var pob = Observable.FromAsync(() => PirateRequest.OpenAsync(new Uri(site.ListUrl), cancellationToken));
            var document = pob.Wait();
            var cells = document.QuerySelectorAll(site.ListItemSelector).ToList();

            var entries = cells.Select(m => new TorrentEntry
            {
                Title = m.TextContent,
                Quality = TorrentQualityExtractor.ExtractQuality(m.TextContent),
                TorrentPage = site.PageBaseUrl + m.GetAttribute("href")?.Trim(),
                TorrentUri = new Uri(site.PageBaseUrl + m.GetAttribute("href")?.Trim())
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
