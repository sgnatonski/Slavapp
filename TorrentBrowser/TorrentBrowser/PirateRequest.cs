using System;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Network;

namespace TorrentBrowser
{
    public static class PirateRequest
    {
        public static IBrowsingContext Context { get; }

        static PirateRequest()
        {
            var config = Configuration.Default.WithDefaultLoader();
            Context = BrowsingContext.New(config);
        }

        public static DocumentRequest Build(string uri)
        {
            var documentRequest = new DocumentRequest(new Url(uri));
            documentRequest.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            documentRequest.Headers["Accept-Charset"] = "utf-8";
            documentRequest.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            return documentRequest;
        }

        public static async Task<IDocument> OpenAsync(Uri uri, CancellationToken cancellationToken)
        {
            return await Context.OpenAsync(Build(uri.AbsoluteUri), cancellationToken);
        }
    }
}
