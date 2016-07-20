using Caliburn.Micro;
using SlavApp.Minion.Resembler.Messages;
using SlavApp.Minion.Resembler.Actions;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SlavApp.Windows;

namespace SlavApp.Minion.Resembler.ViewModels
{
    public class MainViewModel : Screen, IHandle<CancelProgressMessage>, IHandle<FileDeletedMessage>, IHandleWithCoroutine<SearchDirectoryMessage>
    {
        private readonly SimilarityRunAction sAction;
        private readonly GetHashesAction ghAction;
        private readonly FindCopiesAction fcAction;
        private readonly IEventAggregator eventAggregator;
        private readonly JsonSettings.Settings settings;
        private ResultViewModel selected;
        private List<DirectoryCopyResultModel> copyDirs;
        private List<ResultViewModel> list;
        private int calculatedPHashesCount;

        public MainViewModel(IEventAggregator eventAggregator, SimilarityRunAction sAction, GetHashesAction ghAction, FindCopiesAction fcAction, IPathProvider pathProvider)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            this.sAction = sAction;
            this.sAction.Completed += a_Completed;
            this.ghAction = ghAction;
            this.ghAction.Completed += GhAction_Completed;
            this.fcAction = fcAction;
            this.fcAction.Completed += FcAction_Completed;

            this.settings = new JsonSettings.Settings(Path.Combine(pathProvider.BasePath, "settings.json"));

            Coroutine.BeginExecute(RunGH());
        }

        private void FcAction_Completed(object sender, ResultCompletionEventArgs e)
        {
            this.CopyDirs = this.fcAction.CopyDirectories;
        }

        private void GhAction_Completed(object sender, ResultCompletionEventArgs e)
        {
            CalculatedPHashesCount = ghAction.HashesCount;
        }

        private IEnumerator<IResult> RunGH()
        {
            yield return this.ghAction;
        }

        public string DirectoryName
        {
            get
            {
                var dir = settings["directory"];
                if (Directory.Exists(dir))
                {
                    return dir;
                }
                return string.Empty;
            }
            set
            {
                settings.ChangeSetting("directory", value);
                settings.Save();
                NotifyOfPropertyChange(() => DirectoryName);
            }
        }     

        public int SimLevel
        {
            get
            {
                var simLevel = settings["simLevel"];
                if (simLevel == "simLevel")
                {
                    settings.ChangeSetting("simLevel", 3.ToString());
                    settings.Save();
                }
                return int.Parse(settings["simLevel"]);
            }
            set
            {
                var val = value;
                if (val > 20)
                    val = 20;
                if (val < 0)
                    val = 0;
                settings.ChangeSetting("simLevel", val.ToString());
                settings.Save();
                NotifyOfPropertyChange(() => SimLevel);
            }
        }

        public int CalculatedPHashesCount
        {
            get { return this.calculatedPHashesCount; }
            set
            {
                this.calculatedPHashesCount = value;
                NotifyOfPropertyChange(() => CalculatedPHashesCount);
            }
        }

        public List<DirectoryCopyResultModel> CopyDirs
        {
            get { return this.copyDirs; }
            set
            {
                this.copyDirs = value;
                NotifyOfPropertyChange(() => CopyDirs);
            }
        }

        public List<ResultViewModel> List
        {
            get { return this.list; }
            set
            {
                this.list = value;
                NotifyOfPropertyChange(() => List);
            }
        }

        public ResultViewModel Selected
        {
            get { return selected; }
            set
            {
                this.selected = value;
                NotifyOfPropertyChange(() => Selected);
            }
        }

        public void SelectDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = this.DirectoryName;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.DirectoryName = dialog.SelectedPath;
            }
        }

        public IResult ShowAll()
        {
            return null;
        }

        public IResult FindCopies()
        {
            this.fcAction.ProgressViewModel = (IProgressViewModel)this.Parent;
            return this.fcAction;
        }

        public IResult Run()
        {            
            this.sAction.DirectoryName = this.DirectoryName;
            this.sAction.MaxDistance = this.SimLevel;
            this.sAction.Algorithm = settings["algorithm"];
            this.sAction.ProgressViewModel = (IProgressViewModel)this.Parent;

            this.List = new List<ResultViewModel>();
            this.HideSimilarImages();

            return this.sAction;
        }

        private void a_Completed(object sender, ResultCompletionEventArgs e)
        {
            this.sAction.CanRun = false;
            this.List = this.sAction.Result;
        }

        public override void CanClose(Action<bool> callback)
        {
            if (!this.sAction.CanRun)
            {
                callback(true);
            }
            else
            {
                this.sAction.CanRun = false;
                this.sAction.Completed += (sender, ea) =>
                {
                    callback(true);
                };
            }
        }

        public void Handle(CancelProgressMessage message)
        {
            this.sAction.CanRun = false;
        }

        public void ShowSimilarImages(ResultViewModel model)
        {
            this.Selected = model;
        }

        public void HideSimilarImages()
        {
            this.Selected = null;
        }

        public IEnumerable<IResult> Handle(SearchDirectoryMessage message)
        {
            if (message.Directories != null && message.Directories.Any())
            {
                this.DirectoryName = message.Directories[0];

                yield return Run();
            }
        }

        public void Handle(FileDeletedMessage message)
        {
            foreach (var item in this.List)
            {
                RemoveModel(message, item.Similar);
            }

            var rem = this.List.Where(x => x.Similar.Count <= 1).ToList();
            this.List = new List<ResultViewModel>(this.List.Except(rem));
        }

        private static void RemoveModel(FileDeletedMessage message, IList<SimilarityViewModel> items)
        {
            var removeVM = items.FirstOrDefault(x => x.Name == message.FileName);
            if (removeVM != null)
            {
                items.Remove(removeVM);
            }
        }
    }
}
