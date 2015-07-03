using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.Plugin
{
    public interface IProgressViewModel
    {
        Task ShowProgress();
        Task UpdateProgress(string message, double current, double total);
        Task CloseProgress();
    }
}
