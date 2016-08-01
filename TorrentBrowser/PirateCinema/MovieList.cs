using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using TorrentBrowser;

namespace PirateCinema
{
    public class MovieList
    {
        public MovieList(ReactiveList<TorrentMovie> list)
        {
            List = list;
        }

        public ReactiveList<TorrentMovie> List { get; }

        public IReactiveDerivedList<TorrentMovie> ByTitle
        {
            get
            {
                return List.CreateDerivedCollection(
                    x => x,
                    x => true,
                    (x, y) => string.Compare(x.Movie, y.Movie, StringComparison.Ordinal));
            }
        }

        public IReactiveDerivedList<TorrentMovie> ByRating
        {
            get
            {
                return List.CreateDerivedCollection(
                    x => x,
                    x => true,
                    (x, y) => y.Rating.CompareTo(x.Rating));
            }
        }

        public IReactiveDerivedList<TorrentMovie> ByRatingWithSubtitle
        {
            get
            {
                return List.CreateDerivedCollection(
                    x => x,
                    x => x.HasSubtitles,
                    (x, y) => y.Rating.CompareTo(x.Rating));
            }
        }

        public IReactiveDerivedList<TorrentMovie> ByQuality
        {
            get
            {
                return List.CreateDerivedCollection(
                    x => x,
                    x => true,
                    (x, y) => string.Compare(y.Quality, x.Quality, StringComparison.Ordinal));
            }
        }
    }
}
