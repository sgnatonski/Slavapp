using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace TorrentBrowser
{
    public class TorrentMovieCache
    {
        static TorrentMovieCache()
        {
            BsonMapper.Global.EmptyStringToNull = false;
            BsonMapper.Global.RegisterType<Uri>
            (
                serialize: (uri) => uri.AbsoluteUri,
                deserialize: (bson) => new Uri(bson.AsString)
            );

            using (var db = new LiteDatabase(@"pcinema.db"))
            {
                var movies = db.GetCollection<TorrentMovie>("TorrentMovie").Find(Query.GTE("LastUpdated", DateTime.Now.AddDays(-1)));
                imdbCache = new ConcurrentDictionary<string, TorrentMovie>(movies.ToDictionary(x => x.ImdbLink.AbsoluteUri));
            }
        }

        private static readonly ConcurrentDictionary<string, TorrentMovie> imdbCache = new ConcurrentDictionary<string, TorrentMovie>();

        public IEnumerable<TorrentMovie> GetAll()
        {
            return imdbCache.Values;
        }

        public TorrentMovie Get(Uri uri)
        {
            if (imdbCache.ContainsKey(uri.AbsoluteUri))
            {
                var cacheMovie = imdbCache[uri.AbsoluteUri];
                Console.WriteLine($"{cacheMovie.Movie} IMDB rating: {cacheMovie.Rating} [from cache]");
                return cacheMovie;
            }

            return null;
        }

        public void Add(Uri uri, TorrentMovie movie)
        {
            imdbCache.TryAdd(uri.AbsoluteUri, movie);
            using (var db = new LiteDatabase(@"pcinema.db"))
            {
                var c = db.GetCollection<TorrentMovie>("TorrentMovie");
                db.BeginTrans();
                if (!c.Update(movie))
                {
                    c.Insert(movie);
                }
                db.Commit();
            }
        }
    }
}
