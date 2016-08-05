using System;

namespace TorrentBrowser
{
    public static class TorrentMovieSourceFactory
    {
        public static TorrentMovieSource CreateCompleteTorrentMovieSource(TorrentMovie movie)
        {
            return new TorrentMovieSource
            {
                TorrentMovie = movie,
                State = TorrentMovieState.Complete
            };
        }

        public static TorrentMovieSource CreateCompleteTorrentMovieSource(TorrentMovie movie, ImdbData imdbData, string[] subtitles)
        {
            return new TorrentMovieSource
            {
                TorrentMovie = TorrentMovieFactory.CreateTorrentMovie(movie, imdbData, subtitles),
                State = TorrentMovieState.Complete
            };
        }

        public static TorrentMovieSource CreateIncompleteTorrentMovieSource(TorrentEntry torrentEntry, TorrentImdbEntry torrentImdbEntry)
        {
            return new TorrentMovieSource
            {
                TorrentMovie = TorrentMovieFactory.CreateTorrentMovie(torrentEntry, torrentImdbEntry),
                State = TorrentMovieState.Incomplete
            };
        }

        public static TorrentMovieSource CreateInvalidTorrentMovieSource(TorrentEntry torrentEntry)
        {
            return new TorrentMovieSource
            {
                TorrentMovie = TorrentMovieFactory.CreateTorrentMovie(torrentEntry),
                State = TorrentMovieState.Invalid
            };
        }

    }
}