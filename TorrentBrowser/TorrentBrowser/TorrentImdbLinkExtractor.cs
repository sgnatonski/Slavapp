using System;
using System.Linq;
using AngleSharp.Dom;

namespace TorrentBrowser
{
    public static class TorrentImdbLinkExtractor
    {
        public static Uri ExtractImdbLink(IDocument document)
        {
            var links = document.QuerySelectorAll("a");
            var tmp = links.Select(x => x.GetAttribute("href")?.Trim('\r', '\n')).ToList();
            var imdbLink = tmp.FirstOrDefault(x => x != null && x.StartsWith("http://www.imdb.com/title/"));

            if (string.IsNullOrEmpty(imdbLink))
            {
                return null;
            }

            return new Uri(imdbLink.Replace("reference", "").TrimEnd('/') + "/");
        }

        public static int ExtractImdbId(Uri uri)
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
