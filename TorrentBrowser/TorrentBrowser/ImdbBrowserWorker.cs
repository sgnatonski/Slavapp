using System;
using System.Reactive.Linq;
using System.Threading;

namespace TorrentBrowser
{
    public class ImdbBrowserWorker
    {
        private readonly TorrentMovieCachedRepository _torrentRepository;
        private readonly ImdbMovieRepository _imdbRepository;

        private readonly SubtitleLanguage _subtitleLang;

        public ImdbBrowserWorker(SubtitleLanguage subtitleLang)
        {
            _subtitleLang = subtitleLang;
            _torrentRepository = new TorrentMovieCachedRepository();
            _imdbRepository = new ImdbMovieRepository();
        }

        public IObservable<TorrentMovie> Work(IObservable<TorrentMovieSource> torrents, CancellationToken cancellationToken)
        {
            var movies = torrents.Select(torrent => GetMovie(torrent, cancellationToken).Wait());

            return movies;            
        }

        private IObservable<TorrentMovie> GetMovie(TorrentMovieSource torrent, CancellationToken cancellationToken)
        {
            return Observable.FromAsync(async () =>
            {
                if (torrent.State != TorrentMovieState.Incomplete)
                {
                    return torrent.TorrentMovie;
                }

                var imdbData = _imdbRepository.GetById(torrent.TorrentMovie.Id)
                                ?? await ImdbDataExtractor.ExtractData(torrent.TorrentMovie.Id, cancellationToken);

                _imdbRepository.Add(imdbData);

                var subtitles = await OpenSubtitles.GetSubtitles(torrent.TorrentMovie.Id, _subtitleLang);

                var movie = TorrentMovieFactory.CreateTorrentMovie(torrent.TorrentMovie, imdbData, subtitles);

                _torrentRepository.Add(movie.ImdbLink, movie);

                return movie;
            });            
        }
    }
}
