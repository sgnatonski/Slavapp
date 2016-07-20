using Caliburn.Micro;
using SlavApp.Resembler;
using SlavApp.Resembler.DHash;
using SlavApp.Resembler.PHash;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlavApp.Minion.Resembler.Actions
{
    public class InstallContextAction : IResult
    {
        public InstallContextAction()
        {
        }

        public JsonSettings.Settings Settings { get; set; }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };
        public void Execute(CoroutineExecutionContext context)
        {
            var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(InstallContextAction)).Location);
            var psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(path, "SlavApp.Minion.ElevatedRunner.exe");
            psi.Verb = "runas";
            if (Settings["context"] == "1")
            {
                psi.Arguments = "context uninstall";
                Settings["context"] = "0";
            }
            else
            {
                psi.Arguments = "context install";
                Settings["context"] = "1";
            }

            var process = new Process();
            process.StartInfo = psi;
            process.Start();
            process.WaitForExit();
            Settings.Save();
            Completed(this, new ResultCompletionEventArgs());
        }
    }
}
