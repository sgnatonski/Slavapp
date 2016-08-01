using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TorrentBrowser
{
    public class TorrentMovieCachedRepository
    {
        private static bool _initialized;
        private static ConcurrentDictionary<string, TorrentMovie> _imdbCache = new ConcurrentDictionary<string, TorrentMovie>();

        private readonly TorrentMovieRepository _repository;

        public TorrentMovieCachedRepository()
        {
            _repository = new TorrentMovieRepository();
        }

        public IEnumerable<TorrentMovie> GetAll()
        {
            TryInitialize();
            return _imdbCache.Values;
        }

        public TorrentMovie Get(Uri uri)
        {
            TryInitialize();
            TorrentMovie cacheMovie = null;
            _imdbCache.TryGetValue(uri.AbsoluteUri, out cacheMovie);
            return cacheMovie;
        }

        public void Add(Uri uri, TorrentMovie movie)
        {
            _imdbCache.TryAdd(uri.AbsoluteUri, movie);
            _repository.Add(uri, movie);
        }

        private void TryInitialize()
        {
            if (!_initialized)
            {
                _imdbCache = new ConcurrentDictionary<string, TorrentMovie>(_repository.GetAll().ToDictionary(x => x.ImdbLink.AbsoluteUri));
                _initialized = true;
            }
        }
    }
}
