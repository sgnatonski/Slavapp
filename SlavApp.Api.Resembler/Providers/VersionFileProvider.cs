using Newtonsoft.Json;
using SlavApp.Api.Resembler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SlavApp.Api.Resembler.Providers
{
    public class VersionFileProvider: IVersionProvider
    {
        private readonly string fileVersionPath;

        public VersionFileProvider()
        {
            fileVersionPath = HostingEnvironment.MapPath(@"~/App_Data/version.json");
        }
        public async Task<VersionModel> GetVersion()
        {
            using (var reader = File.OpenText(fileVersionPath))
            {
                return JsonConvert.DeserializeObject<VersionModel>(await reader.ReadToEndAsync());
            }
        }
    }
}