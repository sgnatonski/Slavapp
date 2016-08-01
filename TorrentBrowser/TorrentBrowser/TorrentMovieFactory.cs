using System;
using System.Linq;

namespace TorrentBrowser
{
    public static class TorrentMovieFactory
    {
        public static TorrentMovie CreateTorrentMovie(TorrentEntry torrentEntry, TorrentImdbEntry torrentImdbEntry, ImdbData imdbData, string[] subtitles)
        {
            return new TorrentMovie
            {
                Id = torrentImdbEntry.ImdbId,
                TorrentLink = torrentEntry.TorrentUri,
                ImdbLink = torrentImdbEntry.ImdbLink,
                PictureUrl = imdbData.PictureLink,
                Movie = (imdbData.MovieName ?? torrentEntry.Title).Trim(),
                Rating = imdbData.Rating.GetValueOrDefault(),
                Quality = torrentEntry.Quality,
                Subtitles = subtitles,
                LastUpdated = DateTime.Now
            };
        }

        public static TorrentMovie CreateTorrentMovie(TorrentEntry torrentEntry)
        {
            return new TorrentMovie
            {
                TorrentLink = torrentEntry.TorrentUri,
                Movie = torrentEntry.TorrentPage.Split('/').LastOrDefault(),
                Quality = torrentEntry.Quality
            };
        }
    }
}
