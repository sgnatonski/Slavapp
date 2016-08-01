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
                var pages = TorrentList.GetTorrents(site, cancellationToken);

                var movies = pages.AsParallel().Select(async p =>
                {
                    var page = await PirateRequest.OpenAsync(p.TorrentUri, cancellationToken);
                    var imdbUri = TorrentImdbLinkExtractor.ExtractImdbLink(page);

                    if (imdbUri == null)
                    {
                        return new TorrentMovie
                        {
                            TorrentLink = p.TorrentUri,
                            Movie = p.TorrentPage.Split('/').LastOrDefault(),
                            Quality = TorrentQualityExtractor.ExtractQuality(p.Title)
                        };
                    }

                    var cacheMovie = _cache.Get(imdbUri);
                    if (cacheMovie != null)
                    {
                        return cacheMovie;
                    }
                    
                    var imdbData = await ImdbDataExtractor.ExtractData(imdbUri, cancellationToken);
                    
                    var movie = new TorrentMovie
                    {
                        Id = imdbData.Id,
                        TorrentLink = p.TorrentUri,
                        ImdbLink = imdbUri,
                        PictureUrl = imdbData.PictureLink,
                        Movie = (imdbData.MovieName ?? p.Title).Trim(),
                        Rating = imdbData.Rating.GetValueOrDefault(),
                        Quality = TorrentQualityExtractor.ExtractQuality(p.Title),
                        Subtitles = OpenSubtitles.GetSubtitles(imdbData.Id, "pol"),
                        LastUpdated = DateTime.Now
                    };

                    if (imdbData.Id == 0)
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

                return Disposable.Empty;
            });
        }
    }
}
