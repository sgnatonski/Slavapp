using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlavApp.Api.Resembler.Services
{
    public interface IUsageHashDecryptService
    {
        UsageHash DecryptHash(string hash);
    }
}