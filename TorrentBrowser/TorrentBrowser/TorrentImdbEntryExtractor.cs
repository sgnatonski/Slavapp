using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentBrowser
{
    public static class TorrentImdbEntryExtractor
    {
        public static async Task<TorrentImdbEntry> ExtractImdbEntry(Uri uri, CancellationToken cancellationToken)
        {
            var document = await PirateRequest.OpenAsync(uri, cancellationToken);
            var links = document.QuerySelectorAll("a");
            var tmp = links.Select(x => x.GetAttribute("href")?.Trim('\r', '\n')).ToList();
            var imdbLink = tmp.FirstOrDefault(x => x != null && x.StartsWith("http://www.imdb.com/title/"));

            if (string.IsNullOrEmpty(imdbLink))
            {
                return new TorrentImdbEntry();
            }

            var imdbUri = new Uri(imdbLink.Replace("reference", "").TrimEnd('/') + "/");

            return new TorrentImdbEntry
            {
                ImdbLink = imdbUri,
                ImdbId = ExtractImdbId(imdbUri)
            };
        }

        private static int ExtractImdbId(Uri uri)
        {
            var id = 0;
            if (uri == null)
            {
                return id;
            }

            int.TryParse(uri.Segments[2].Substring(2).TrimEnd('/'), out id);

            return id;
        }
    }
}
