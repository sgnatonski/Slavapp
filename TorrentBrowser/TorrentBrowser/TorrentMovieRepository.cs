using System;
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
        }

        public IEnumerable<TorrentMovie> GetAll()
        {
            using (var db = new LiteDatabase(PCinemaDbName))
            {
                var movies = db.GetCollection<TorrentMovie>(TorrentMovieCollectionName).Find(Query.GTE("LastUpdated", DateTime.Now.AddDays(-1)));
                return movies.ToList();
            }
        }

        public void Add(Uri uri, TorrentMovie movie)
        {
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
