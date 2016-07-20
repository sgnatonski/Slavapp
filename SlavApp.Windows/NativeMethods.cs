using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Windows
{
    internal static class NativeMethods
    {
        /// <summary>
        /// EXTERN
        /// The SwitchToThisWindow function is called to switch focus to a specified window and bring it to the foreground.
        /// </summary>
        /// <param name="windowHandle">Handle to the window being switched to.</param>
        /// <param name="altTab">A TRUE for this parameter indicates that the window is being switched to using the Alt/Ctrl+Tab key sequence. This parameter should be FALSE otherwise.</param>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern void SwitchToThisWindow(IntPtr windowHandle, bool altTab);

        /// <summary>
        /// The ShowWindowAsync function sets the show state of a window created by a different thread.
        /// </summary>
        /// <param name="windowHandle">Handle to the window.</param>
        /// <param name="showWindowCommand">Specifies how the window is to be shown. For a list of possible values, see the description of the ShowWindow function.</param>
        /// <returns>The asynchronous handle</returns>
        [DllImport("User32.dll")]
        internal static extern int ShowWindowAsync(IntPtr windowHandle, int showWindowCommand);

        /// <summary>
        /// The IsWindowVisible function retrieves the visibility state of the specified window.
        /// </summary>
        /// <param name="windowHandle">Handle to the window to test.</param>
        /// <returns>
        /// If the specified window, its parent window, its parent's parent window, and so forth, have the WS_VISIBLE style, the return value is nonzero. 
        /// Otherwise, the return value is zero. Because the return value specifies whether the window has the WS_VISIBLE style, it may be nonzero even if 
        /// the window is totally obscured by other windows.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool IsWindowVisible(IntPtr windowHandle);
    }
}
