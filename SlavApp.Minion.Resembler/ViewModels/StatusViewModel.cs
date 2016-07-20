using Caliburn.Micro;
using SlavApp.Minion.Resembler.Actions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SlavApp.Minion.Resembler.ViewModels
{
    public class StatusViewModel : Screen
    {
        private readonly ImageScanAction action;

        public StatusViewModel(ImageScanAction action)
        {
            this.action = action;
            this.Files = new ObservableCollection<string>();
            this.Files.CollectionChanged += Files_CollectionChanged;
            this.action.DirSearched += OnDirectorySearching;
            this.action.FileFound += OnFileFound;
            this.action.FileAnalyzed += OnFileAnalyzed;
            this.action.FileSkipped += OnFileSkipped;
            this.action.Completed += Action_Completed;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Coroutine.ExecuteAsync(Run());
        }

        private IEnumerator<IResult> Run()
        {
            this.IsRunning = true;
            this.Files.Clear();
            yield return this.action;
        }

        private void OnFileAnalyzed(object sender, EventArgs e)
        {
            IsAnalyzing = true;
            FilesAnalyzed++;
        }

        private void OnFileSkipped(object sender, EventArgs e)
        {
            IsAnalyzing = true;
            FilesAnalyzed++;
            FilesSkipped++;
        }

        private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => FilesFound);
        }

        private void Action_Completed(object sender, ResultCompletionEventArgs e)
        {
            this.IsRunning = false;
        }

        private void OnFileFound(object sender, FileFoundEventArgs e)
        {
            this.Files.Add(e.File);
        }

        private void OnDirectorySearching(object sender, DirSearchedEventArgs e)
        {
            this.CurrentMessage = e.Directory;
        }

        private ObservableCollection<string> files;
        public ObservableCollection<string> Files
        {
            get { return files; }
            set { files = value; NotifyOfPropertyChange(() => Files); }
        }

        public int FilesFound
        {
            get { return this.Files.Count; }
        }

        private int filesAnalyzed;
        public int FilesAnalyzed
        {
            get { return filesAnalyzed; }
            set { filesAnalyzed = value; NotifyOfPropertyChange(() => FilesAnalyzed); }
        }

        private int filesSkipped;
        public int FilesSkipped
        {
            get { return filesSkipped; }
            set { filesSkipped = value; NotifyOfPropertyChange(() => FilesSkipped); }
        }

        private bool isRunning;
        public bool IsRunning
        {
            get { return isRunning; }
            set { isRunning = value; NotifyOfPropertyChange(() => IsRunning); }
        }

        private bool isAnalyzing;
        public bool IsAnalyzing
        {
            get { return isAnalyzing; }
            set { isAnalyzing = value; NotifyOfPropertyChange(() => IsAnalyzing); NotifyOfPropertyChange(() => IsCounting); }
        }

        public bool IsCounting
        {
            get { return !this.IsAnalyzing; }
        }

        private string currentMessage;
        public string CurrentMessage
        {
            get { return currentMessage; }
            set { currentMessage = value; NotifyOfPropertyChange(() => CurrentMessage); }
        }
    }
}
