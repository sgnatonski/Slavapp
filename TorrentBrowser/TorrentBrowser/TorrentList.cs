using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;

namespace TorrentBrowser
{
    public static class TorrentList
    {
        public static IEnumerable<TorrentEntry> GetTorrents(TorrentSite site, CancellationToken cancellationToken)
        {
            var pob = Observable.FromAsync(() => PirateRequest.OpenAsync(new Uri(site.ListUrl), cancellationToken));
            var document = pob.Wait();
            var cells = document.QuerySelectorAll(site.ListItemSelector).ToList();

            var entries = cells.Select(m => new TorrentEntry
            {
                Title = m.TextContent,
                TorrentPage = site.PageBaseUrl + m.GetAttribute("href")?.Trim(),
                TorrentUri = new Uri(site.PageBaseUrl + m.GetAttribute("href")?.Trim())
            });

            return FilterMovies(entries);
        }

        private static IEnumerable<TorrentEntry> FilterMovies(IEnumerable<TorrentEntry> movies)
        {
            return movies
                        .Where(p => p.TorrentPage != null
                        && !p.TorrentPage.ToLower().Contains("dvdscr")
                        && !p.TorrentPage.ToLower().Contains("camrip")
                        && !p.TorrentPage.ToLower().Contains("hdcam")
                        && !p.TorrentPage.ToLower().Contains(".tc.")
                        && !p.TorrentPage.ToLower().Contains("hdtc")
                        && !p.TorrentPage.ToLower().Contains("hdts")
                        && !p.TorrentPage.ToLower().Contains("hd-ts"));
        }
    }
}
