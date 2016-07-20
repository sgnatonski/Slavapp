using Newtonsoft.Json;
using SlavApp.Api.Resembler.Models;
using SlavApp.Api.Resembler.Providers;
using SlavApp.Api.Resembler.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using SlavApp.Api.Resembler.Extensions;

namespace SlavApp.Api.Resembler.Controllers
{
    public class VersionController : ApiController
    {
        private readonly IVersionProvider _versionProvider;
        private readonly IResemblerUsageService _usageService;

        public VersionController(IVersionProvider versionProvider, IResemblerUsageService usageService)
        {
            _versionProvider = versionProvider;
            _usageService = usageService;
        }

        [HttpGet]
        [Route("version")]
        public async Task<string> Get(string hash = null)
        {
            await _usageService.RegisterUsage(Request.GetClientIpAddress().ToString(), hash);

            var model = await _versionProvider.GetVersion();
            return model.V;
        }

        [HttpGet]
        [Route("version/url")]
        public async Task<string> GetDownloadUrl()
        {
            var model = await _versionProvider.GetVersion();
            return model.V;
        }
    }
}
