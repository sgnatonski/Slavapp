using System;
using System.Linq;

namespace TorrentBrowser
{
    public class TorrentMovieSource
    {
        public TorrentMovie TorrentMovie { get; set; }
        public TorrentMovieState State { get; set; }

    }

    public enum TorrentMovieState
    {
        Complete,
        Incomplete,
        Invalid
    }
}
