using System;
using System.Linq;
using LiteDB;

namespace TorrentBrowser
{
    public class TorrentImdbEntryRepository
    {
        private const string PCinemaDbName = "pcinema.db";
        private const string TorrentImdbEntryCollectionName = "TorrentImdbEntry";
                
        public TorrentImdbEntry GetById(Uri id)
        {
            using (var db = new LiteDatabase(PCinemaDbName))
            {
                var movie = db.GetCollection<TorrentImdbEntry>(TorrentImdbEntryCollectionName)
                    .Find(x => x.TorrentLink == id)
                    .FirstOrDefault(x => x.TorrentLink == id);
                return movie;
            }
        }

        public void Add(TorrentImdbEntry movie)
        {
            using (var db = new LiteDatabase(PCinemaDbName))
            {
                var c = db.GetCollection<TorrentImdbEntry>(TorrentImdbEntryCollectionName);
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
