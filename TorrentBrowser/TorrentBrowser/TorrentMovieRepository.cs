using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace TorrentBrowser
{
    public class TorrentMovieRepository
    {
        private const string PCinemaDbName = "pcinema.db";
        private const string TorrentMovieCollectionName = "TorrentMovie";

        static TorrentMovieRepository()
        {
            BsonMapper.Global.EmptyStringToNull = false;
            BsonMapper.Global.RegisterType(uri => uri.AbsoluteUri, bson => new Uri(bson.AsString));

            using (var db = new LiteDatabase(PCinemaDbName))
            {
                var movies = db.GetCollection<TorrentMovie>(TorrentMovieCollectionName).Find(Query.GTE("LastUpdated", DateTime.Now.AddDays(-1)));
                ImdbCache = new ConcurrentDictionary<string, TorrentMovie>(movies.ToDictionary(x => x.ImdbLink.AbsoluteUri));
            }
        }

        private static readonly ConcurrentDictionary<string, TorrentMovie> ImdbCache = new ConcurrentDictionary<string, TorrentMovie>();

        public IEnumerable<TorrentMovie> GetAll()
        {
            return ImdbCache.Values;
        }

        public TorrentMovie Get(Uri uri)
        {
            if (!ImdbCache.ContainsKey(uri.AbsoluteUri))
            {
                return null;
            }

            var cacheMovie = ImdbCache[uri.AbsoluteUri];
            return cacheMovie;
        }

        public void Add(Uri uri, TorrentMovie movie)
        {
            ImdbCache.TryAdd(uri.AbsoluteUri, movie);
            using (var db = new LiteDatabase(PCinemaDbName))
            {
                var c = db.GetCollection<TorrentMovie>(TorrentMovieCollectionName);
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
