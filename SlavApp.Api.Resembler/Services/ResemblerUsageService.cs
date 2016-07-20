using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using SlavApp.Api.Resembler.Models;
using System.Threading.Tasks;
using SlavApp.Api.Resembler.Providers;
using System.Net;

namespace SlavApp.Api.Resembler.Services
{
    public class ResemblerUsageService : IResemblerUsageService
    {
        private readonly ICloudTableProvider _cloudTableProvider;
        private readonly IUsageHashDecryptService _usageHashDecryptService;

        public ResemblerUsageService(ICloudTableProvider cloudTableProvider, IUsageHashDecryptService usageHashDecryptService)
        {
            _cloudTableProvider = cloudTableProvider;
            _usageHashDecryptService = usageHashDecryptService;
        }

        public async Task RegisterUsage(string ipAddress, string hash)
        {
            try
            {
                var usageHash = _usageHashDecryptService.DecryptHash(hash);
                if (usageHash == null)
                {
                    // log error
                    return;
                }

                var table = await _cloudTableProvider.GetTable("resemblerusage");

                var tableOperation = TableOperation.Insert(new UsageEntity()
                {
                    PartitionKey = usageHash.Version,
                    RowKey = Guid.NewGuid().ToString(),
                    Timestamp = DateTimeOffset.Now,
                    IpAddress = ipAddress,
                    Identity = usageHash.Identity,
                    PHashCount = usageHash.PHashCount,
                    DHashCount = usageHash.DHashCount
                });

                var result = await table.ExecuteAsync(tableOperation);
                if (result.HttpStatusCode < 200 && result.HttpStatusCode >= 300)
                {
                    // log error
                }
            }
            catch (StorageException)
            {
                // log error
            }
            catch (Exception)
            {
                // log error
            }
        }

        private class UsageEntity : TableEntity
        {
            public string IpAddress { get; set; }

            public string Identity { get; set; }

            public int PHashCount { get; set; }

            public int DHashCount { get; set; }
        }
    }
}