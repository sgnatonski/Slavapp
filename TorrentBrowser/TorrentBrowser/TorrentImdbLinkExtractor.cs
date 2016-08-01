using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
