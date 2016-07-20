using SlavApp.Minion.Resembler.Context.Handlers.Shell;
using SlavApp.Resembler;
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

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!ApplicationHelper.CheckApplicationAlreadyRunning("SlavApp.Minion", true) && !ApplicationHelper.CheckApplicationAlreadyRunning("SlavApp.Minion.vshost", true))
            {
                var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(CompareImagesContextMenuHandler)).Location);
                var psi = new ProcessStartInfo();
                psi.FileName = Path.Combine(path, "..\\..\\..\\SlavApp.Minion\\bin\\Debug\\SlavApp.Minion.exe");
                psi.WorkingDirectory = Path.GetDirectoryName(psi.FileName);
                var process = new Process();
                process.StartInfo = psi;
                process.Start();
                process.WaitForInputIdle();
            }
            NamedPipeListener<string>.SendMessage("SlavApp.Minion.Resembler", "boo");

            Console.ReadLine();
            return;

            /*var current = 0.0;
            var sf = new SimilarHistogramFinder();
            sf.OnCompareProgress += (total, file1, file2, v) =>
            {
                //lock(lockObj)
                {
                    current++;
                    Console.WriteLine("{0:0.00} % ({1} / {2})", (current / total) * 100.0, current, total);
                }
            };

            sf.Run(@"R:\APART_ALL\ZDJĘCIA EXPO", "*.jpg", 0.6);

            Console.ReadLine();*/

            //Debug.WriteLine(results.Count());
            //results.OrderByDescending(x => x.Value).ToList().ForEach(x => Debug.WriteLine("{0}\t\t{1}\t\t{2} %", Path.GetFileName(x.First), Path.GetFileName(x.Second), x.Value * 100));
        }

        static object lockObj = new object();
    }
}
