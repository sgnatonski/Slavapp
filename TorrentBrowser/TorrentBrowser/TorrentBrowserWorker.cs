using AngleSharp;
using AngleSharp.Network;
using AngleSharp.Parser.Html;
using AngleSharp.Scripting;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TorrentBrowser
{
    public class TorrentBrowserWorker
    {
        private class MovieTitle
        {
            public string Title { get; set; }
            public string Page { get; set; }
        }

        static TorrentBrowserWorker()
        {
            BsonMapper.Global.EmptyStringToNull = false;
            BsonMapper.Global.RegisterType<Uri>
            (
                serialize: (uri) => uri.AbsoluteUri,
                deserialize: (bson) => new Uri(bson.AsString)
            );

            using (var db = new LiteDatabase(@"pcinema.db"))
            {
                var movies = db.GetCollection<TorrentMovie>("TorrentMovie").Find(Query.GTE("LastUpdated", DateTime.Now.AddDays(-1)));
                imdbCache = new ConcurrentDictionary<string, TorrentMovie>(movies.ToDictionary(x => x.ImdbLink.AbsoluteUri));
            }
        }

        private static readonly ConcurrentDictionary<string, TorrentMovie> imdbCache = new ConcurrentDictionary<string, TorrentMovie>();

        public IEnumerable<TorrentMovie> GetCache()
        {
            return imdbCache.Values;
        }

        public IObservable<TorrentMovie> Work(TorrentSite site, CancellationToken cancellationToken)
        {
            return Observable.Create<TorrentMovie>(observer =>
            {                
                var config = Configuration.Default.WithDefaultLoader();
                var browsingContext = BrowsingContext.New(config);
                var pob = Observable.FromAsync(() => browsingContext.OpenAsync(CreateRequest(site.ListUrl), cancellationToken));
                var document = pob.Wait();                
                var cells = document.QuerySelectorAll(site.ListItemSelector).ToList();

                if (!cells.Any())
                {
                    observer.OnCompleted();
                    return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
                }

                var pages = cells.Select(m => new MovieTitle { Title = m.TextContent, Page = m.GetAttribute("href")?.Trim() });
                var movies = FilterMovies(pages).AsParallel().Select(async p =>
                {
                    var torrentLink = site.PageBaseUrl + p.Page;
                    var page = await browsingContext.OpenAsync(CreateRequest(torrentLink), cancellationToken);
                    var links = page.QuerySelectorAll("a");
                    var tmp = links.Select(x => x.GetAttribute("href")?.Trim('\r', '\n')).ToList();
                    var imdbLink = tmp.Where(x => x != null && x.StartsWith("http://www.imdb.com/title/")).FirstOrDefault();

                    if (imdbLink == null)
                    {
                        return new TorrentMovie
                        {
                            TorrentLink = new Uri(torrentLink),
                            Movie = p.Page.Split('/').LastOrDefault(),
                            Quality = GetQuality(p.Title)
                        };
                    }

                    var imdbUri = new Uri(imdbLink.Replace("reference", "").TrimEnd('/') + "/");

                    if (imdbCache.ContainsKey(imdbUri.AbsoluteUri))
                    {
                        var cacheMovie = imdbCache[imdbUri.AbsoluteUri];
                        Console.WriteLine($"{cacheMovie.Movie} IMDB rating: {cacheMovie.Rating} [from cache]");
                        return cacheMovie;
                    }

                    var imdbPage = await browsingContext.OpenAsync(CreateRequest(imdbUri.AbsoluteUri), cancellationToken);
                    var movieName = imdbPage.QuerySelector("#title-overview-widget > div.vital > div.title_block > div > div.titleBar > div.title_wrapper > div.originalTitle")?.TextContent.Replace("(original title)", "")
                                 ?? imdbPage.QuerySelector("#title-overview-widget > div.vital > div.title_block > div > div.titleBar > div.title_wrapper > h1")?.TextContent;
                    var rating = imdbPage.QuerySelector("#title-overview-widget > div.vital > div.title_block > div > div.ratings_wrapper > div.imdbRating > div.ratingValue > strong > span")?.TextContent;
                    var pictureUrl = imdbPage.QuerySelector("#title-overview-widget > div.vital > div.slate_wrapper > div.poster > a > img")?.GetAttribute("src");
                    Console.WriteLine($"{(movieName ?? p.Title).Trim()} IMDB rating: {rating ?? "no IMDB data"}");

                    var id = 0;
                    int.TryParse(imdbUri.Segments[2].Substring(2).TrimEnd('/'), out id);

                    var subs = new string[0];
                    try
                    {
                        var osXml = XDocument.Load("http://www.opensubtitles.org/en/search/imdbid-" + id +"/sublanguageid-pol/xml", LoadOptions.None);
                        subs = osXml.XPathSelectElements("//opensubtitles/search/results/subtitle/IDSubtitle").Select(x => x.Attribute("LinkDownload").Value).ToArray();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    
                    var movie = new TorrentMovie
                    {
                        Id = id,
                        TorrentLink = torrentLink != null ? new Uri(torrentLink) : null,
                        ImdbLink = imdbUri,
                        PictureUrl = pictureUrl != null ? new Uri(pictureUrl) : null,
                        Movie = (movieName ?? p.Title).Trim(),
                        Rating = float.Parse(rating ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                        Quality = GetQuality(p.Title),
                        Subtitles = subs,
                        LastUpdated = DateTime.Now
                    };

                    if (id == 0)
                    {
                        return movie;
                    }

                    imdbCache.TryAdd(imdbUri.AbsoluteUri, movie);
                    using (var db = new LiteDatabase(@"pcinema.db"))
                    {
                        var c = db.GetCollection<TorrentMovie>("TorrentMovie");
                        db.BeginTrans();
                        if (!c.Update(movie))
                        {
                            c.Insert(movie);
                        }
                        db.Commit();
                    }

                    return movie;
                });

                movies.Select(t => Observable.FromAsync(() => t))
                      .Merge()
                      .Do(s => observer.OnNext(s), () => observer.OnCompleted())
                      .Wait();

                return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
            });
        }

        private string GetQuality(string moviename)
        {
            var m = moviename.ToLower().Split(' ', '.', '_', '(', ')', '[', ']');
            var tags = new[] 
            {
                "dvdscr", "camrip", "hdcam", "tc", "hdtc", "hdts", "hd-ts",
                "hdrip", "hd-rip", "dvdrip", "brrip", "bdrip", "webrip"
            };

            return string.Join(" ", m.Intersect(tags)) ?? string.Empty;
        }

        private IEnumerable<MovieTitle> FilterMovies(IEnumerable<MovieTitle> movies)
        {
            return movies
                        .Where(p => p.Page != null
                        && !p.Page.ToLower().Contains("dvdscr")
                        && !p.Page.ToLower().Contains("camrip")
                        && !p.Page.ToLower().Contains("hdcam")
                        && !p.Page.ToLower().Contains(".tc.")
                        && !p.Page.ToLower().Contains("hdtc")
                        && !p.Page.ToLower().Contains("hdts")
                        && !p.Page.ToLower().Contains("hd-ts"));
        }

        private DocumentRequest CreateRequest(string uri)
        {
            var documentRequest = new DocumentRequest(new Url(uri));
            documentRequest.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            documentRequest.Headers["Accept-Charset"] = "utf-8";
            documentRequest.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            return documentRequest;
        }
    }
}
