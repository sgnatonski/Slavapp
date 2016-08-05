namespace TorrentBrowser
{
    public static class TorrentMovieSourceFactory
    {
        public static TorrentMovieSource GetCompleteTorrentMovieSource(TorrentMovie movie)
        {
            return new TorrentMovieSource
            {
                TorrentMovie = movie,
                State = TorrentMovieState.Incomplete
            };
        }

        public static TorrentMovieSource GetIncompleteTorrentMovieSource(TorrentEntry torrentEntry, TorrentImdbEntry torrentImdbEntry)
        {
            return new TorrentMovieSource
            {
                TorrentMovie = TorrentMovieFactory.CreateTorrentMovie(torrentEntry, torrentImdbEntry),
                State = TorrentMovieState.Complete
            };
        }

        public static TorrentMovieSource GetInvalidTorrentMovieSource(TorrentEntry torrentEntry)
        {
            return new TorrentMovieSource
            {
                TorrentMovie = TorrentMovieFactory.CreateTorrentMovie(torrentEntry),
                State = TorrentMovieState.Invalid
            };
        }
    }
}