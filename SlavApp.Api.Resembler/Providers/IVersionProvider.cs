using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SlavApp.Api.Resembler.Models;

namespace SlavApp.Api.Resembler.Providers
{
    public interface IVersionProvider
    {
        Task<VersionModel> GetVersion();
    }
}