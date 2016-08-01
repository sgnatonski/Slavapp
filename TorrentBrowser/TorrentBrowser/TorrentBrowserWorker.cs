﻿using System;
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
                    var quality = TorrentQualityExtractor.ExtractQuality(p.Title);
                    var page = await PirateRequest.OpenAsync(p.TorrentUri, cancellationToken);
                    var imdbUri = TorrentImdbLinkExtractor.ExtractImdbLink(page);
                    var imdbId = TorrentImdbLinkExtractor.ExtractImdbId(imdbUri);

                    if (imdbUri == null || imdbId == 0)
                    {
                        return new TorrentMovie
                        {
                            TorrentLink = p.TorrentUri,
                            Movie = p.TorrentPage.Split('/').LastOrDefault(),
                            Quality = quality
                        };
                    }
                    
                    var cacheMovie = _repository.Get(imdbUri);
                    if (cacheMovie != null)
                    {
                        Console.WriteLine($"[from cache] {cacheMovie.Movie} IMDB rating: {cacheMovie.Rating}");
                        return cacheMovie;
                    }
                    
                    var imdbData = await ImdbDataExtractor.ExtractData(imdbUri, cancellationToken);
                    var subtitles = await OpenSubtitles.GetSubtitles(imdbId, "pol");

                    var movie = new TorrentMovie
                    {
                        Id = imdbData.Id,
                        TorrentLink = p.TorrentUri,
                        ImdbLink = imdbUri,
                        PictureUrl = imdbData.PictureLink,
                        Movie = (imdbData.MovieName ?? p.Title).Trim(),
                        Rating = imdbData.Rating.GetValueOrDefault(),
                        Quality = quality,
                        Subtitles = subtitles,
                        LastUpdated = DateTime.Now
                    };

                    _repository.Add(imdbUri, movie);

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
