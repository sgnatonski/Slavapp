using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace TorrentBrowser
{
    public class TorrentBrowserWorker
    {
        private readonly TorrentMovieCachedRepository _torrentRepository;
        private readonly ImdbMovieRepository _imdbRepository;

        private readonly SubtitleLanguage _subtitleLang;

        public TorrentBrowserWorker(SubtitleLanguage subtitleLang)
        {
            _subtitleLang = subtitleLang;
            _torrentRepository = new TorrentMovieCachedRepository();
            _imdbRepository = new ImdbMovieRepository();
        }

        public IEnumerable<TorrentMovie> GetCache()
        {
            return _torrentRepository.GetAll();
        }

        public IObservable<TorrentMovie> Work(TorrentSite site, CancellationToken cancellationToken)
        {
            var torrents = TorrentList.GetTorrents(site, cancellationToken).Result;
            var movies = torrents.Select(torrent => GetMovie(torrent, cancellationToken).Wait());

            return movies;            
        }

        private IObservable<TorrentMovie> GetMovie(TorrentEntry torrent, CancellationToken cancellationToken)
        {
            return Observable.FromAsync(async () =>
            {
                var imdbEntry = await TorrentImdbEntryExtractor.ExtractImdbEntry(torrent.TorrentUri, cancellationToken);
                
                if (!imdbEntry.IsValid)
                {
                    return TorrentMovieFactory.CreateTorrentMovie(torrent);
                }
                
                var cacheMovie = _torrentRepository.Get(imdbEntry.ImdbLink);
                if (cacheMovie != null)
                {
                    Console.WriteLine($"[from cache] {cacheMovie.Movie} IMDB rating: {cacheMovie.Rating}");
                    return cacheMovie;
                }

                var imdbData = _imdbRepository.GetById(imdbEntry.ImdbId)
                                ?? await ImdbDataExtractor.ExtractData(imdbEntry.ImdbId, cancellationToken);
                var subtitles = await OpenSubtitles.GetSubtitles(imdbEntry.ImdbId, _subtitleLang);

                var movie = TorrentMovieFactory.CreateTorrentMovie(torrent, imdbEntry, imdbData, subtitles);

                _imdbRepository.Add(imdbData);
                _torrentRepository.Add(movie.ImdbLink, movie);

                return movie;
            });            
        }
    }
}
