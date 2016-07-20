using SharpShell.Attributes;
using SharpShell.Diagnostics;
using SharpShell.ServerRegistration;
using SharpShell.SharpContextMenu;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlavApp.Minion.Resembler.Context.Handlers.Shell
{
    /// <summary>
    /// The CountLinesExtensions is an example shell context menu extension,
    /// implemented with SharpShell. It adds the command 'Count Lines' to text
    /// files.
    /// </summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    //[COMServerAssociation(AssociationType.ClassOfExtension, ".jpg")]
    public class CompareImagesContextMenuHandler : SharpContextMenu
    {
        private readonly ExplorerConfigurationManager explorerConfigurationManager = new ExplorerConfigurationManager() { AlwaysUnloadDll = true };
        public void Register()
        {
            ServerRegistrationManager.InstallServer(this, RegistrationType.OS64Bit, true);
            ServerRegistrationManager.RegisterServer(this, RegistrationType.OS64Bit);
        }

        public void Unregister()
        {
            ServerRegistrationManager.UnregisterServer(this, RegistrationType.OS64Bit);
            ServerRegistrationManager.UninstallServer(this, RegistrationType.OS64Bit);
            ExplorerManager.RestartExplorer();
        }

        protected override bool CanShowMenu()
        {
            //  We always show the menu.
            return this.SelectedItemPaths != null && this.SelectedItemPaths.Any();
        }
        
        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            
            var itemCountLines = new ToolStripMenuItem
            {
                Text = "Search similar images (local)"
            };
            
            itemCountLines.Click += (sender, args) => CompareImagesLocally();
            
            menu.Items.Add(itemCountLines);
            
            return menu;
        }

        private void CompareImagesLocally()
        {
            if (!ApplicationHelper.CheckApplicationAlreadyRunning("SlavApp.Minion", true) && !ApplicationHelper.CheckApplicationAlreadyRunning("SlavApp.Minion.vshost", true))
            {
                var path = Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Assembly.GetAssembly(typeof(CompareImagesContextMenuHandler)).Location)).FullName).FullName;
                var psi = new ProcessStartInfo();
                psi.FileName = Path.Combine(path, "SlavApp.Minion.exe");
                psi.WorkingDirectory = path;
                psi.WindowStyle = ProcessWindowStyle.Normal;
                Process.Start(psi).WaitForInputIdle(1500);
            }
            Task.WaitAny(new[] { Task.Run(() =>
            {
                var dirs = this.SelectedItemPaths.Where(x => File.GetAttributes(x).HasFlag(FileAttributes.Directory));
                NamedPipeListener<string>.SendMessage("SlavApp.Minion.Resembler", string.Join(Environment.NewLine, dirs));
            }) }, 1000);
        }
    }
}
