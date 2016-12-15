using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TorrentBrowser;

namespace PirateCinema
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

        public MainViewModel()
        {
            Application.Current.Exit += Current_Exit;

            FetchCommand = ReactiveCommand.CreateAsyncObservable(FetchMovies);
            SelectedLangId = SubtitleLanguage.FromCurrentCulture();

            InitializeCachedList();
        }

        public ICommand FetchCommand { get; set; }

        [Reactive]
        public IReadOnlyReactiveList<TorrentMovie> MoviesList { get; set; }

        [Reactive]
        public string SelectedLangId { get; set; }

        public IObservable<TorrentMovie> FetchMovies(object param)
        {
            var movieList = MovieListFactory.Build();

            MoviesList = movieList.ByRatingWithSubtitle;
            
            var observable = Observable.Start(() =>
            {
                var torrentWorker = new TorrentWorkerFacade(SubtitleLanguage.FromIsoName(SelectedLangId), _cancelToken.Token);

                return torrentWorker.Work(
                    TorrentSiteProvider.PirateBay,
                    TorrentSiteProvider.ExtraTorrent,
                    TorrentSiteProvider._1337x);
            }).Merge();

            observable
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(movieList.List.Add, _cancelToken.Token);

            return observable;
        }

        private void InitializeCachedList()
        {
            var movies = MovieListFactory.Build(new TorrentBrowserWorker().GetCache());

            MoviesList = movies.ByRatingWithSubtitle;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            _cancelToken?.Cancel();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                _cancelToken.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
