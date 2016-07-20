using SlavApp.Api.Resembler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SlavApp.Api.Resembler.Services
{
    public interface IResemblerUsageService
    {
        Task RegisterUsage(string ipAddress, string hash);
    }
}