using SlavApp.Minion.Resembler.Context.Handlers.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ElevatedRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            if (args[0] == "context")
            {
                Console.Write("Context ");
                if (args[1] == "install")
                {
                    Console.WriteLine("installing...");
                    new CompareImagesContextMenuHandler().Register();
                }
                if (args[1] == "uninstall")
                {
                    Console.WriteLine("uninstalling...");
                    new CompareImagesContextMenuHandler().Unregister();
                }
            }

            Console.WriteLine("Done. Press any key to close.");
            Console.ReadLine();
        }
    }
}
