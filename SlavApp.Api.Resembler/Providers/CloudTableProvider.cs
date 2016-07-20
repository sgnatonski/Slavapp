using System.Web.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace SlavApp.Api.Resembler.Providers
{
    public class CloudTableProvider : ICloudTableProvider
    {
        public async Task<CloudTable> GetTable(string tablename)
        {
            var account = new CloudStorageAccount(new StorageCredentials("slavapp", WebConfigurationManager.AppSettings["AzureStorageKey"]), true);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(tablename);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}