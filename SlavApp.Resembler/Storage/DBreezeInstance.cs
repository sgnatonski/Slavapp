using DBreeze;
using DBreeze.Transactions;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Resembler.Storage
{
    public class DBreezeInstance : IDisposable
    {
        private readonly DBreezeEngine engine;

        public DBreezeInstance(IPathProvider pathProvider)
        {
            var dbConf = new DBreezeConfiguration()
            {
                DBreezeDataFolderName = Path.Combine(pathProvider.BasePath, "DBR"),
                Storage = DBreezeConfiguration.eStorage.DISK
            };
            this.engine = new DBreezeEngine(dbConf);
        }

        public Transaction GetTransaction()
        {
            return this.engine.GetTransaction();
        }

        public void Dispose()
        {
            if (this.engine != null)
            {
                this.engine.Dispose();
            }
        }
    }
}
