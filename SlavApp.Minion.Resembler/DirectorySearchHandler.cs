using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using SlavApp.Minion.Resembler.Messages;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlavApp.Minion.Resembler
{
    public class DirectorySearchHandler
    {
        private readonly IEventAggregator eventAggregator;
        private NamedPipeListener<string> pipeListener;
        private DateTime lastTimestamp = DateTime.Now;
        private int timeWindow = 100;

        public DirectorySearchHandler(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void Initialize(IPlugin plugin)
        {
            pipeListener = new NamedPipeListener<string>("SlavApp.Minion.Resembler");
            pipeListener.MessageReceived += (sender, e) => { OnMessageReceived(plugin, e); };
            pipeListener.Error += (sender, e) => { OnError(e); };
            pipeListener.Start();
        }

        private void OnMessageReceived(IPlugin plugin, NamedPipeListenerMessageReceivedEventArgs<string> e)
        {
            var timestamp = DateTime.Now;
            if ((timestamp - lastTimestamp).Milliseconds < timeWindow)
            {
                return;
            }
            lastTimestamp = timestamp;

            this.eventAggregator.PublishOnUIThread(new OpenPluginMessage()
            {
                Plugin = plugin
            });

            var dirs = e.Message
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => File.GetAttributes(x).HasFlag(FileAttributes.Directory))
                .ToArray();

            this.eventAggregator.PublishOnUIThread(new SearchDirectoryMessage()
            {
                Directories = dirs
            });
        }

        private static void OnError(NamedPipeListenerErrorEventArgs e)
        {
            MessageBox.Show(string.Format("Error ({0}): {1}", e.ErrorType, e.Exception.Message));
        }

        public void Dispose()
        {
            pipeListener.End();
        }
    }
}
