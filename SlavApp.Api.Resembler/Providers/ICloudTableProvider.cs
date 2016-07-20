using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SlavApp.Api.Resembler.Providers
{
    public interface ICloudTableProvider
    {
        Task<CloudTable> GetTable(string tablename);
    }
}