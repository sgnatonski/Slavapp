using System;
using System.Linq;
using System.Diagnostics;

namespace SlavApp.Windows
{
    public static class ApplicationHelper
    {
        /// <summary>
        /// Checks whether another instance of the same application is already running.
        /// </summary>
        /// <param name="switchToAlreadyRunningProcess">Whether the already running process is flashed and brought to front.</param>
        /// <returns>Whether another instance of the application is already running.</returns>
        public static bool CheckApplicationAlreadyRunning(string appName, bool switchToAlreadyRunningProcess)
        {
            //Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            Process[] processes = Process.GetProcessesByName(appName);
            
            if (processes.Length > 0)
            {
                if (switchToAlreadyRunningProcess)
                {
                    IntPtr hwnd = processes[0].Id != Process.GetCurrentProcess().Id ? processes[0].MainWindowHandle : processes[1].MainWindowHandle;

                    if (!NativeMethods.IsWindowVisible(hwnd))
                    {
                        NativeMethods.ShowWindowAsync(hwnd, 1); // maximize window
                    }

                    NativeMethods.SwitchToThisWindow(hwnd, true);
                }

                return true;
            }

            return false;
        }
    }
}
