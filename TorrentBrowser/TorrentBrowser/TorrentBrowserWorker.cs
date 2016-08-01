using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace TorrentBrowser
{
    public class TorrentBrowserWorker
    {
        private class MovieTitle
        {
            public string Title { get; set; }
            public string Page { get; set; }
        }

        private readonly TorrentMovieCache _cache;

        public TorrentBrowserWorker()
        {
            _cache = new TorrentMovieCache();
        }
        
        public IEnumerable<TorrentMovie> GetCache()
        {
            return _cache.GetAll();
        }

        public IObservable<TorrentMovie> Work(TorrentSite site, CancellationToken cancellationToken)
        {
            return Observable.Create<TorrentMovie>(observer =>
            {                
                var config = Configuration.Default.WithDefaultLoader();
                var browsingContext = BrowsingContext.New(config);
                var pob = Observable.FromAsync(() => browsingContext.OpenAsync(PirateRequestFactory.Build(site.ListUrl), cancellationToken));
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
                    var page = await browsingContext.OpenAsync(PirateRequestFactory.Build(torrentLink), cancellationToken);
                    var links = page.QuerySelectorAll("a");
                    var tmp = links.Select(x => x.GetAttribute("href")?.Trim('\r', '\n')).ToList();
                    var imdbLink = tmp.FirstOrDefault(x => x != null && x.StartsWith("http://www.imdb.com/title/"));

                    if (imdbLink == null)
                    {
                        return new TorrentMovie
                        {
                            TorrentLink = new Uri(torrentLink),
                            Movie = p.Page.Split('/').LastOrDefault(),
                            Quality = QualityExtractor.ExtractQuality(p.Title)
                        };
                    }

                    var imdbUri = new Uri(imdbLink.Replace("reference", "").TrimEnd('/') + "/");

                    var cacheMovie = _cache.Get(imdbUri);
                    if (cacheMovie != null)
                    {
                        return cacheMovie;
                    }

                    var imdbPage = await browsingContext.OpenAsync(PirateRequestFactory.Build(imdbUri.AbsoluteUri), cancellationToken);
                    var imdbQueries = new ImdbQueryProvider();
                    var movieName = imdbPage.QuerySelector(imdbQueries.OriginalTitleQuery)?.TextContent.Replace("(original title)", "")
                                 ?? imdbPage.QuerySelector(imdbQueries.TitleQuery)?.TextContent;
                    var rating = imdbPage.QuerySelector(imdbQueries.RatingQuery)?.TextContent;
                    var pictureUrl = imdbPage.QuerySelector(imdbQueries.PictureQuery)?.GetAttribute("src");
                    Console.WriteLine($"{(movieName ?? p.Title).Trim()} IMDB rating: {rating ?? "no IMDB data"}");

                    var id = 0;
                    int.TryParse(imdbUri.Segments[2].Substring(2).TrimEnd('/'), out id);
                    
                    var movie = new TorrentMovie
                    {
                        Id = id,
                        TorrentLink = torrentLink != null ? new Uri(torrentLink) : null,
                        ImdbLink = imdbUri,
                        PictureUrl = pictureUrl != null ? new Uri(pictureUrl) : null,
                        Movie = (movieName ?? p.Title).Trim(),
                        Rating = float.Parse(rating ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                        Quality = QualityExtractor.ExtractQuality(p.Title),
                        Subtitles = OpenSubtitles.GetSubtitles(id, "pol"),
                        LastUpdated = DateTime.Now
                    };

                    if (id == 0)
                    {
                        return movie;
                    }

                    _cache.Add(imdbUri, movie);

                    return movie;
                });

                movies.Select(t => Observable.FromAsync(() => t))
                      .Merge()
                      .Do(observer.OnNext, observer.OnCompleted)
                      .Wait();

                return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
            });
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
    }
}
