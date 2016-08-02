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
        private readonly TorrentMovieCachedRepository _repository;

        private readonly SubtitleLanguage _subtitleLang;

        public TorrentBrowserWorker(SubtitleLanguage subtitleLang)
        {
            _subtitleLang = subtitleLang;
            _repository = new TorrentMovieCachedRepository();
        }

        public IEnumerable<TorrentMovie> GetCache()
        {
            return _repository.GetAll();
        }

        public IObservable<TorrentMovie> Work(TorrentSite site, CancellationToken cancellationToken)
        {
            return Observable.Create<TorrentMovie>(observer =>
            {
                var torrentsObs = Observable.FromAsync(() => TorrentList.GetTorrents(site, cancellationToken));
                var torrents = torrentsObs.Wait();

                var movies = torrents.AsParallel().Select(async torrent =>
                {
                    var imdbEntry = await TorrentImdbEntryExtractor.ExtractImdbEntry(torrent.TorrentUri, cancellationToken);

                    if (!imdbEntry.IsValid)
                    {
                        return TorrentMovieFactory.CreateTorrentMovie(torrent);
                    }
                    
                    var cacheMovie = _repository.Get(imdbEntry.ImdbLink);
                    if (cacheMovie != null)
                    {
                        Console.WriteLine($"[from cache] {cacheMovie.Movie} IMDB rating: {cacheMovie.Rating}");
                        return cacheMovie;
                    }
                    
                    var imdbData = await ImdbDataExtractor.ExtractData(imdbEntry.ImdbLink, cancellationToken);
                    var subtitles = await OpenSubtitles.GetSubtitles(imdbEntry.ImdbId, _subtitleLang);

                    var movie = TorrentMovieFactory.CreateTorrentMovie(torrent, imdbEntry, imdbData, subtitles);

                    _repository.Add(movie.ImdbLink, movie);

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
