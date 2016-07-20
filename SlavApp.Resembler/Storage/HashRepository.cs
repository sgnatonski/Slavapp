using DBreeze.Transactions;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Resembler.Storage
{
    public class HashRepository
    {
        private readonly DBreezeInstance dbInstance;
        private readonly ILog log;

        public HashRepository(DBreezeInstance dbInstance, ILog log)
        {
            this.dbInstance = dbInstance;
            this.log = log;
        }

        public IEnumerable<string> GetHashedFiles()
        {
            using (var tran = this.dbInstance.GetTransaction())
            {
                return GetHashedFiles(tran);
            }
        }

        public IEnumerable<string> GetHashedFiles(Transaction tran)
        {
            var mm = tran.SelectForward<string, string>("murmur").ToDictionary(x => x.Key, y => y.Value).Flip();
            //var hh = tran.SelectForward<string, ulong>("hash").ToDictionary(x => x.Key, y => y.Value);//.Flip();
            return tran.SelectForward<string, ulong>("hash").SelectMany(x => mm[x.Key]).ToList();
        }

        public Dictionary<string, ulong> GetHashes()
        {
            using (var tran = dbInstance.GetTransaction())
            {
                return GetHashes(tran);
            }
        }

        public Dictionary<string, ulong> GetHashes(Transaction tran)
        {
            return tran.SelectForward<string, ulong>("hash").ToDictionary(x => x.Key, y => y.Value);
        }

        public void RemoveHash(Transaction tran, string murmur)
        {
            try
            {
                tran.RemoveKey("hash", murmur);
            }
            catch (Exception ex)
            {
                this.log.Error("Hash remove failed", ex);
                throw;
            }
        }

        public void InsertHash(Transaction tran, string murmur, ulong hash)
        {
            try
            {
                var pcData = tran.Select<string, ulong>("hash", murmur, true);
                if (pcData.Exists)
                {
                    tran.RemoveKey("hash", murmur);                    
                }
                tran.Insert("hash", murmur, hash);
            }
            catch (Exception ex)
            {
                this.log.Error("Hash insert failed", ex);
                throw;
            }
        }
    }
}
