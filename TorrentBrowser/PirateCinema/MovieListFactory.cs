using System.Collections.Generic;
using ReactiveUI;
using TorrentBrowser;

namespace PirateCinema
{
    public static class MovieListFactory
    {
        public static MovieList Build()
        {
            return new MovieList(new ReactiveList<TorrentMovie>());
        }

        public static MovieList Build(IEnumerable<TorrentMovie> movies)
        {
            return new MovieList(new ReactiveList<TorrentMovie>(movies));
        }
    }
}
