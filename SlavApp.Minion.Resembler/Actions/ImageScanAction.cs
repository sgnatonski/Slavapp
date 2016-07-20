using Caliburn.Micro;
using SlavApp.Minion.Resembler.ViewModels;
using SlavApp.Resembler.DHash;
using SlavApp.Resembler.PHash;
using SlavApp.Resembler.Storage;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SlavApp.Minion.Resembler.Actions
{
    public class ImageScanAction : IResult
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;
        private readonly HashRepository hashes;
        private readonly PHashCalculator phash_simFinder;
        private readonly StartScanViewModel ssVM;
        private readonly JsonSettings.Settings settings;

        private string[] allImages = new[] { "*.jpg", "*.jpeg", "*.jfif", "*.bmp", "*.tif", "*.tiff", "*.png" };

        private SortedSet<string> hashedFiles;

        public ImageScanAction(IEventAggregator eventAggregator, IWindowManager windowManager, HashRepository hashes, PHashCalculator psimFinder, IPathProvider pathProvider,
            StartScanViewModel ssVM)
        {
            this.eventAggregator = eventAggregator;
            this.windowManager = windowManager;
            this.hashes = hashes;
            this.ssVM = ssVM;
            this.phash_simFinder = psimFinder;
            
            this.settings = new JsonSettings.Settings(Path.Combine(pathProvider.BasePath, "settings.json"));
        }

        public bool CanRun { get; set; }
        public bool IsPaused { get; set; }

        public event EventHandler<DirSearchedEventArgs> DirSearched = delegate { };
        public event EventHandler<FileFoundEventArgs> FileFound = delegate { };
        public event EventHandler<EventArgs> FileAnalyzed = delegate { };
        public event EventHandler<EventArgs> FileSkipped = delegate { };
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public void Execute(CoroutineExecutionContext context)
        {
            this.IsPaused = true;
            this.CanRun = windowManager.ShowDialog(ssVM).GetValueOrDefault();
            this.IsPaused = false;

            using (var backgroundWorker = new BackgroundWorker())
            {
                backgroundWorker.DoWork += (e, sender) =>
                {
                    while (IsPaused)
                    {
                        Thread.Sleep(500);
                    }

                    if (CanRun)
                    {
                        this.hashedFiles = new SortedSet<string>(hashes.GetHashedFiles());
                        var files = SearchDrives();
                        phash_simFinder.OnProgress += OnProgress;
                        phash_simFinder.OnSkipProgress += OnSkipProgress;
                        phash_simFinder.Run(files, () => this.CanRun, () => IsPaused);
                    }
                };
                backgroundWorker.RunWorkerCompleted += (e, sender) => 
                    Completed(this, new ResultCompletionEventArgs());
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void OnProgress(string file)
        {
            FileAnalyzed(this, new EventArgs());
        }

        private void OnSkipProgress(string file)
        {
            FileSkipped(this, new EventArgs());
        }

        private List<string> SearchDrives()
        {
            var drives = Directory.GetLogicalDrives();
            var d = settings["drives"];
            var selectedDrives = drives;
            if (d != "drives")
            {
                selectedDrives = d.Split(';');
            }

            var files = new List<string>();
            foreach (string s in selectedDrives.AsParallel())
            {
                Search(s, files);
            }
            return files;
        }

        private void Search(string s, List<string> files)
        {
            while (IsPaused)
            {
                Thread.Sleep(500);
            }

            try
            {
                foreach (string d in Directory.GetDirectories(s))
                {
                    var dirInfo = new DirectoryInfo(d);
                    if (dirInfo.Attributes.HasFlag(FileAttributes.Hidden) ||
                        dirInfo.Attributes.HasFlag(FileAttributes.System) ||
                        dirInfo.Attributes.HasFlag(FileAttributes.Temporary) ||
                        d.StartsWith(@"C:\Windows"))
                    {
                        continue;
                    }

                    DirSearched(this, new DirSearchedEventArgs(d));
                    try
                    {
                        foreach (string f in allImages.SelectMany(ext => Directory.GetFiles(d, ext, SearchOption.TopDirectoryOnly)).Select(x => Pathing.GetUNCPath(x)))
                        {
                            if (this.hashedFiles.Contains(f, EqualityComparer<string>.Default))
                            {
                                continue;
                            }

                            files.Add(f);
                            FileFound(this, new FileFoundEventArgs(f));
                        }

                        foreach (string sub in Directory.GetDirectories(d, "*", SearchOption.TopDirectoryOnly))
                        {
                            Search(sub, files);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    public class DirSearchedEventArgs : EventArgs
    {
        public string Directory { get; private set; }

        public DirSearchedEventArgs(string dir)
        {
            Directory = dir;
        }
    }

    public class FileFoundEventArgs : EventArgs
    {
        public string File { get; private set; }

        public FileFoundEventArgs(string dir)
        {
            File = dir;
        }
    }
}