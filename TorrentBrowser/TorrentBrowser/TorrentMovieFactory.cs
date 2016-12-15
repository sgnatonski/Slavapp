using System;
using System.Linq;

namespace TorrentBrowser
{
    public static class TorrentMovieFactory
    {
        public static TorrentMovie CreateTorrentMovie(TorrentMovie movie, ImdbData imdbData, SubtitleData[] subtitles)
        {
            return new TorrentMovie
            {
                Id = movie.Id,
                TorrentLink = movie.TorrentLink,
                ImdbLink = movie.ImdbLink,
                PictureUrl = imdbData.PictureLink,
                Movie = (imdbData.MovieName ?? movie.Movie).Trim(),
                Rating = imdbData.Rating.GetValueOrDefault(),
                Quality = movie.Quality,
                Subtitles = subtitles,
                LastUpdated = DateTime.Now
            };
        }

        public static TorrentMovie CreateTorrentMovie(TorrentEntry torrentEntry, TorrentImdbEntry torrentImdbEntry)
        {
            return new TorrentMovie
            {
                Id = torrentImdbEntry.ImdbId,
                TorrentLink = torrentEntry.TorrentUri,
                ImdbLink = torrentImdbEntry.ImdbLink,
                Quality = torrentEntry.Quality,
                LastUpdated = DateTime.Now
            };
        }

        public static TorrentMovie CreateTorrentMovie(TorrentEntry torrentEntry)
        {
            return new TorrentMovie
            {
                TorrentLink = torrentEntry.TorrentUri,
                Movie = torrentEntry.TorrentPage.Split('/').LastOrDefault(),
                Quality = torrentEntry.Quality,
                LastUpdated = DateTime.Now
            };
        }
    }
}
