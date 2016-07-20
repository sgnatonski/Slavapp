using DBreeze.Transactions;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Resembler.Storage
{
    public class MurmurRepository
    {
        private readonly DBreezeInstance dbInstance;
        private readonly ILog log;

        public MurmurRepository(DBreezeInstance dbInstance, ILog log)
        {
            this.dbInstance = dbInstance;
            this.log = log;
        }

        public string GetMurMur(Transaction tran, string filename)
        {
            var row = tran.Select<string, string>("murmur", filename.ToLower(), true);
            return row.Exists ? row.Value : null;
        }

        public Dictionary<string, string> GetMurMurs()
        {
            using (var tran = dbInstance.GetTransaction())
            {
                return tran.SelectForward<string, string>("murmur").ToDictionary(x => x.Key, y => y.Value);
            }
        }

        public void InsertMurMur(Transaction tran, string filename, string hash)
        {
            try
            {
                tran.Insert("murmur", filename.ToLower(), hash);
            }
            catch (Exception ex)
            {
                this.log.Error("Hash (murmur) insert failed", ex);
                throw;
            }
        }
        public void RemoveMurMur(Transaction tran, string filename)
        {
            try
            {
                tran.RemoveKey("murmur", filename.ToLower());
            }
            catch (Exception ex)
            {
                this.log.Error("Hash (murmur) remove failed", ex);
                throw;
            }
        }
    }
}
