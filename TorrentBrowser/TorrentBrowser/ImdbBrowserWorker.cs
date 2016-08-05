using System;
using System.Reactive.Linq;
using System.Threading;

namespace TorrentBrowser
{
    public class ImdbBrowserWorker
    {
        private readonly ImdbMovieRepository _imdbRepository;

        private readonly SubtitleLanguage _subtitleLang;

        public ImdbBrowserWorker(SubtitleLanguage subtitleLang)
        {
            _subtitleLang = subtitleLang;
            _imdbRepository = new ImdbMovieRepository();
        }

        public IObservable<TorrentMovieSource> Work(IObservable<TorrentMovieSource> torrents, CancellationToken cancellationToken)
        {
            var movies = torrents.Select(torrent => GetMovie(torrent, cancellationToken).Wait());

            return movies;            
        }

        private IObservable<TorrentMovieSource> GetMovie(TorrentMovieSource torrent, CancellationToken cancellationToken)
        {
            return Observable.FromAsync(async () =>
            {
                if (torrent.State != TorrentMovieState.Incomplete)
                {
                    return torrent;
                }

                var imdbData = _imdbRepository.GetById(torrent.TorrentMovie.Id)
                                ?? await ImdbDataExtractor.ExtractData(torrent.TorrentMovie.Id, cancellationToken);

                _imdbRepository.Add(imdbData);

                var subtitles = await OpenSubtitles.GetSubtitles(torrent.TorrentMovie.Id, _subtitleLang);

                var movie = TorrentMovieSourceFactory.CreateCompleteTorrentMovieSource(torrent.TorrentMovie, imdbData, subtitles);
                
                return movie;
            });            
        }
    }
}
