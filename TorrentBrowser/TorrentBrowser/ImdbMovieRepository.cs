using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace TorrentBrowser
{
    public class ImdbMovieRepository
    {
        private const string PCinemaDbName = "pcinema.db";
        private const string ImdbMovieCollectionName = "ImdbMovie";

        static ImdbMovieRepository()
        {
            BsonMapper.Global.EmptyStringToNull = false;
            BsonMapper.Global.RegisterType(uri => uri.AbsoluteUri, bson => new Uri(bson.AsString));
        }

        public ImdbData GetById(int id)
        {
            using (var db = new LiteDatabase(PCinemaDbName))
            {
                var movie = db.GetCollection<ImdbData>(ImdbMovieCollectionName)
                    .Find(x => x.Id == id && x.LastUpdated >= DateTime.Now.AddDays(-7))
                    .FirstOrDefault();
                return movie;
            }
        }

        public void Add(ImdbData movie)
        {
            movie.LastUpdated = DateTime.Now;

            using (var db = new LiteDatabase(PCinemaDbName))
            using (var trans = db.BeginTrans())
            {
                var c = db.GetCollection<ImdbData>(ImdbMovieCollectionName);
                if (!c.Update(movie))
                {
                    c.Insert(movie);
                }
                trans.Commit();
            }
        }
    }
}
