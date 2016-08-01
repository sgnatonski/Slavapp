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
        private readonly TorrentMovieRepository _repository;

        public TorrentBrowserWorker()
        {
            _repository = new TorrentMovieRepository();
        }
        
        public IEnumerable<TorrentMovie> GetCache()
        {
            return _repository.GetAll();
        }

        public IObservable<TorrentMovie> Work(TorrentSite site, CancellationToken cancellationToken)
        {
            return Observable.Create<TorrentMovie>(observer =>
            {
                var pages = TorrentList.GetTorrents(site, cancellationToken);

                var movies = pages.AsParallel().Select(async p =>
                {
                    var imdbEntry = await TorrentImdbEntryExtractor.ExtractImdbEntry(p.TorrentUri, cancellationToken);

                    if (!imdbEntry.IsValid)
                    {
                        return new TorrentMovie
                        {
                            TorrentLink = p.TorrentUri,
                            Movie = p.TorrentPage.Split('/').LastOrDefault(),
                            Quality = p.Quality
                        };
                    }
                    
                    var cacheMovie = _repository.Get(imdbEntry.ImdbLink);
                    if (cacheMovie != null)
                    {
                        Console.WriteLine($"[from cache] {cacheMovie.Movie} IMDB rating: {cacheMovie.Rating}");
                        return cacheMovie;
                    }
                    
                    var imdbData = await ImdbDataExtractor.ExtractData(imdbEntry.ImdbLink, cancellationToken);
                    var subtitles = await OpenSubtitles.GetSubtitles(imdbEntry.ImdbId, "pol");

                    var movie = new TorrentMovie
                    {
                        Id = imdbEntry.ImdbId,
                        TorrentLink = p.TorrentUri,
                        ImdbLink = imdbEntry.ImdbLink,
                        PictureUrl = imdbData.PictureLink,
                        Movie = (imdbData.MovieName ?? p.Title).Trim(),
                        Rating = imdbData.Rating.GetValueOrDefault(),
                        Quality = p.Quality,
                        Subtitles = subtitles,
                        LastUpdated = DateTime.Now
                    };

                    _repository.Add(imdbEntry.ImdbLink, movie);

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
