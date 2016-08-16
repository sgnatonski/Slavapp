using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;

namespace TorrentBrowser
{
    public class TorrentBrowserWorker
    {
        private readonly TorrentMovieCachedRepository _torrentRepository;
        private readonly TorrentImdbEntryRepository _torrentImdbEntryRepository;

        public TorrentBrowserWorker()
        {
            _torrentRepository = new TorrentMovieCachedRepository();
            _torrentImdbEntryRepository = new TorrentImdbEntryRepository();
        }

        public IEnumerable<TorrentMovie> GetCache()
        {
            return _torrentRepository.GetAll();
        }

        public IObservable<TorrentMovieSource> Work(TorrentSite site, CancellationToken cancellationToken)
        {
            var torrents = TorrentList.GetTorrents(site, cancellationToken).Result;
            var movies = torrents.Select(torrent => GetMovie(torrent, cancellationToken));

            return movies.Merge();            
        }

        public IObservable<TorrentMovieSource> UpdateCache(IObservable<TorrentMovieSource> torrents)
        {
            return torrents.Select(UpdateCache);
        }

        private IObservable<TorrentMovieSource> GetMovie(TorrentEntry torrent, CancellationToken cancellationToken)
        {
            return Observable.FromAsync(async () =>
            {
                var imdbEntry = _torrentImdbEntryRepository.GetById(torrent.TorrentUri)
                    ?? await TorrentImdbEntryExtractor.ExtractImdbEntry(torrent.TorrentUri, cancellationToken);
                
                if (!imdbEntry.IsValid)
                {
                    return TorrentMovieSourceFactory.CreateInvalidTorrentMovieSource(torrent);
                }

                _torrentImdbEntryRepository.Add(imdbEntry);

                var cacheMovie = _torrentRepository.Get(imdbEntry.ImdbLink);
                if (cacheMovie != null)
                {
                    Console.WriteLine($"[from cache] {cacheMovie.Movie} IMDB rating: {cacheMovie.Rating}");
                    return TorrentMovieSourceFactory.CreateCompleteTorrentMovieSource(cacheMovie);
                }

                return TorrentMovieSourceFactory.CreateIncompleteTorrentMovieSource(torrent, imdbEntry);
            });            
        }

        private TorrentMovieSource UpdateCache(TorrentMovieSource torrent)
        {
            if (torrent.State == TorrentMovieState.Complete)
            {
                _torrentRepository.Add(torrent.TorrentMovie.ImdbLink, torrent.TorrentMovie);
            }
            return torrent;
        }
    }
}
