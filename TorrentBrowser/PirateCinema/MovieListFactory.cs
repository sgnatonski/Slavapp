using System;
using System.Collections.Generic;
using System.Linq;
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

        public static MovieList Build(ReactiveList<TorrentMovie> movies)
        {
            return new MovieList(movies);
        }
    }
}
